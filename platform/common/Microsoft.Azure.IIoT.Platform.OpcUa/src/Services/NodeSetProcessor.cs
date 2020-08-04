// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Services {
    using Opc.Ua;
    using Opc.Ua.Encoders;
    using Opc.Ua.Extensions;
    using Opc.Ua.Nodeset;
    using Serilog;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Process from stream and call node handlers
    /// </summary>
    public class NodeSetProcessor : INodeSetProcessor {

        /// <summary>
        /// Create source stream importer
        /// </summary>
        /// <param name="codec"></param>
        /// <param name="logger"></param>
        public NodeSetProcessor(IVariantEncoderFactory codec, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        }

        /// <summary>
        /// Bulk publish
        /// </summary>
        /// <param name="body"></param>
        /// <param name="handler"></param>
        /// <param name="contentType"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ProcessAsync(Stream body, INodeHandler handler,
            string contentType, CancellationToken ct) {

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            if (contentType == null) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (body == null) {
                throw new ArgumentNullException(nameof(body));
            }

            if (contentType.EndsWith("+gzip")) {
                body = new GZipStream(body, CompressionMode.Decompress, true);
                contentType = contentType.Replace("+gzip", "");
            }

            var context = _codec.Default.Context.ToSystemContext();
            try {
                if (contentType == ContentMimeType.UaNodesetXml) {
                    // If nodeset, read as nodeset xml
                    var nodeset = NodeSet2.Load(body);
                    foreach (var node in nodeset.GetNodeStates(context)) {
                        ct.ThrowIfCancellationRequested();
                        if (node != null) {
                            await HandleAsync(handler, node, context);
                        }
                    }
                }
                else {
                    // Otherwise decode from opc ua encoded stream
                    using (var decoder = new ModelDecoder(body, contentType,
                            _codec.Default.Context)) {
                        while (true) {
                            ct.ThrowIfCancellationRequested();
                            var node = decoder.ReadEncodeable<EncodeableNodeModel>(null);
                            if (node == null) {
                                break;
                            }
                            await HandleAsync(handler, node.Node, context);
                        }
                    }
                }
                // Commit
                await handler.CompleteAsync(context);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to parse model, aborting processing.");
                // Abort
                await handler.CompleteAsync(context, true);
            }
        }

        /// <summary>
        /// Convert node to node vertex model
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task HandleAsync(INodeHandler handler, BaseNodeModel node,
            ISystemContext context) {
            if (node == null) {
                return;
            }
            switch (node) {
                case ObjectNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case PropertyNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case VariableNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case MethodNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case ViewNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case ObjectTypeNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case PropertyTypeNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case VariableTypeNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case DataTypeNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
                case ReferenceTypeNodeModel n:
                    await handler.HandleAsync(n, context);
                    break;
            }
        }

        private readonly ILogger _logger;
        private readonly IVariantEncoderFactory _codec;
    }
}