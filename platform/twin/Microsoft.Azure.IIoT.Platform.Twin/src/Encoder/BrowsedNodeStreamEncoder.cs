// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using Opc.Ua.Encoders;
    using Opc.Ua.Extensions;
    using Opc.Ua.Nodeset;
    using Opc.Ua.Client;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Encode browsed nodes to stream.
    /// </summary>
    public sealed class BrowsedNodeStreamEncoder : IDisposable {

        /// <inheritdoc/>
        public IEnumerable<string> HistoryNodes => _history.Values;

        /// <inheritdoc/>
        public List<OperationResultModel> Diagnostics { get; } =
            new List<OperationResultModel>();

        /// <summary>
        /// Create node stream encoder
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpoint"></param>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="diagnostics"></param>
        /// <param name="logger"></param>
        /// <param name="priority"></param>
        public BrowsedNodeStreamEncoder(IConnectionServices client, ConnectionModel endpoint,
            Stream stream, string contentType, DiagnosticsModel diagnostics, ILogger logger,
            int priority = int.MaxValue) :
            this(client, endpoint, diagnostics, logger,  priority) {
            _encoder = new ModelEncoder(stream, contentType, PushNode);
        }

        /// <summary>
        /// Create node stream encoder
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpoint"></param>
        /// <param name="encoder"></param>
        /// <param name="diagnostics"></param>
        /// <param name="logger"></param>
        /// <param name="priority"></param>
        public BrowsedNodeStreamEncoder(IConnectionServices client, ConnectionModel endpoint,
            IEncoder encoder, DiagnosticsModel diagnostics, ILogger logger,
            int priority = int.MaxValue) :
            this(client, endpoint, diagnostics, logger, priority) {
            _encoder = new ModelEncoder(encoder, PushNode);
        }

        /// <summary>
        /// Create node stream encoder
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpoint"></param>
        /// <param name="diagnostics"></param>
        /// <param name="logger"></param>
        /// <param name="priority"></param>
        private BrowsedNodeStreamEncoder(IConnectionServices client, ConnectionModel endpoint,
            DiagnosticsModel diagnostics, ILogger logger, int priority) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _diagnostics = diagnostics;
            _priority = priority;
            _browseStack.Push(ObjectIds.RootFolder);
            _browseStack.Push(ObjectIds.TypesFolder);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _encoder.Dispose();
        }

        /// <inheritdoc/>
        public async Task EncodeAsync(CancellationToken ct) {
            if (_visited.Count > 0) {
                throw new InvalidOperationException("Encoding already performed.");
            }
            _logger.LogDebug("Encoding all nodes in address space ...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (_browseStack.Count > 0) {
                ct.ThrowIfCancellationRequested();
                var nodeId = _browseStack.Pop();
                if (!NotSeen(nodeId)) {
                    continue;
                }
                var node = await ReadNodeAsync(nodeId, ct).ConfigureAwait(false);
                if (node == null) {
                    continue;
                }
                await FetchReferencesAsync(node, ct).ConfigureAwait(false);
                // Write encodeable node
                _encoder.WriteEncodeable(null, new EncodeableNodeModel(node));
                _nodes++;
            }
            _logger.LogDebug("Encoded {nodes} nodes and {references} references " +
                "in address space in {elapsed}...", _nodes, _references, sw.Elapsed);
        }

        private int _nodes;
        private int _references;

        /// <summary>
        /// Push node into browse stack
        /// </summary>
        /// <param name="nodeId"></param>
        private void PushNode(ExpandedNodeId nodeId) {
            if ((nodeId?.ServerIndex ?? 1u) != 0) {
                return;
            }
            var local = (NodeId)nodeId;
            if (NotSeen(local)) {
                _browseStack.Push(local);
            }
        }

        /// <summary>
        /// Check whether node was seen already
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        private bool NotSeen(NodeId local) {
            return
                !NodeId.IsNull(local) &&
                !_visited.Contains(local) &&
                !_history.ContainsKey(local);
        }

        /// <summary>
        /// Load references
        /// </summary>
        /// <param name="nodeModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        internal async Task FetchReferencesAsync(BaseNodeModel nodeModel, CancellationToken ct) {
            try {
                // Read node with value
                var response = await _client.ExecuteServiceAsync(_endpoint, _priority, session => {
                        _encoder.Context.UpdateFromSession(session);
                        return session.BrowseAsync(_diagnostics.ToStackModel(), null,
                            nodeModel.NodeId, 0u, Opc.Ua.BrowseDirection.Both,
                            ReferenceTypeIds.References, true, 0u);
                    }, ct).ConfigureAwait(false);

                SessionClientEx.Validate(response.Results, response.DiagnosticInfos);
                OperationResultEx.Validate("Browse_" + nodeModel.NodeId, Diagnostics,
                    response.Results.Select(r => r.StatusCode), null, false);
                while (true) {
                    foreach (var reference in response.Results[0].References) {
                        nodeModel.AddReference(reference.ReferenceTypeId,
                            !reference.IsForward, reference.NodeId);
                        _references++;
                    }
                    if (response.Results[0].ContinuationPoint == null) {
                        break;
                    }
                    response = await _client.ExecuteServiceAsync(_endpoint, _priority, session => {
                        _encoder.Context.UpdateFromSession(session);
                        return session.BrowseNextAsync(_diagnostics.ToStackModel(), false,
                            new ByteStringCollection { response.Results[0].ContinuationPoint });
                    }, ct).ConfigureAwait(false);
                    SessionClientEx.Validate(response.Results, response.DiagnosticInfos);
                    OperationResultEx.Validate("BrowseNext_" + nodeModel.NodeId, Diagnostics,
                        response.Results.Select(r => r.StatusCode), null, false);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed browsing node object for node {nodeId}.",
                    nodeModel.NodeId);
            }
        }

        /// <summary>
        /// Read node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<BaseNodeModel> ReadNodeAsync(NodeId nodeId, CancellationToken ct) {
            try {
                // Read node with value
                return await _client.ExecuteServiceAsync(_endpoint, _priority, async session => {
                        _encoder.Context.UpdateFromSession(session);
                        var node = await RawNodeModel.ReadAsync(session, _diagnostics.ToStackModel(),
                            nodeId, false, Diagnostics, false, ct).ConfigureAwait(false);
                        // Determine whether to read events or historic data later
                        if (node.IsHistorizedNode) {
                            _history.Add(node.LocalId, node.NodeId.AsString(
                                session.MessageContext));
                        }
                        else {
                            // Otherwise mark as visited so we do not browse again.
                            _visited.Add(nodeId);
                        }
                        var isProperty =
                            (node.NodeClass == Opc.Ua.NodeClass.Variable ||
                             node.NodeClass == Opc.Ua.NodeClass.VariableType) &&
                             session.TypeTree.IsTypeOf(node.NodeId, VariableTypeIds.PropertyType);
                        return node.ToNodeModel(isProperty);
                    }, ct).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed reading node object for node {nodeId}.", nodeId);
                _visited.Add(nodeId);
                return null;
            }
        }

        private readonly int _priority;
        private readonly IConnectionServices _client;
        private readonly ConnectionModel _endpoint;
        private readonly DiagnosticsModel _diagnostics;
        private readonly ModelEncoder _encoder;
        private readonly ILogger _logger;

        private readonly Stack<NodeId> _browseStack =
            new Stack<NodeId>();
        private readonly HashSet<NodeId> _visited =
            new HashSet<NodeId>();
        private readonly Dictionary<NodeId, string> _history =
            new Dictionary<NodeId, string>();
    }
}