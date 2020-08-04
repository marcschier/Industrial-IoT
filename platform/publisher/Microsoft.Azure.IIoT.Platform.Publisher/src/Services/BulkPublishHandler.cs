// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Opc.Ua;
    using Opc.Ua.Extensions;
    using Opc.Ua.Nodeset;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Bulk publisher
    /// </summary>
    public class BulkPublishHandler : IBulkPublishHandler {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="publish"></param>
        public BulkPublishHandler(INodeSetProcessor processor, IPublishServices publish) {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _publish = publish ?? throw new ArgumentNullException(nameof(publish));
        }

        /// <inheritdoc/>
        public async Task PublishFromNodesetAsync(string endpointId, string sessionId,
            Stream nodeset, string contentType, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (string.IsNullOrEmpty(sessionId)) {
                throw new ArgumentNullException(nameof(sessionId));
            }
            if (string.IsNullOrEmpty(contentType)) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (nodeset == null) {
                throw new ArgumentNullException(nameof(nodeset));
            }
            var session = new PublishingSession(this, endpointId, sessionId);
            await _processor.ProcessAsync(nodeset, session, contentType, ct);
        }

        /// <summary>
        /// Handles adding variables to published nodes job
        /// </summary>
        private class PublishingSession : NodeHandlerBase {

            public PublishingSession(BulkPublishHandler outer, string endpointId,
                string sessionId) {
                _endpointId = endpointId;
                _sessionId = sessionId;
                _outer = outer;
            }

            /// <inheritdoc/>
            public override async Task CompleteAsync(ISystemContext context,
                bool abort = false) {
                if (_cache.Count != 0) {
                    await PublishFromCacheAsync(context);
                }
            }

            /// <inheritdoc/>
            public override async Task HandleAsync(VariableNodeModel node,
                ISystemContext context) {
                _cache.Add(node);
                if (_cache.Count >= kMaxCacheSize) {
                    await PublishFromCacheAsync(context);
                }
            }

            /// <summary>
            /// Add all items in cache
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            private async Task PublishFromCacheAsync(ISystemContext context) {
                var ctx = context.ToMessageContext();
                await _outer._publish.NodePublishBulkAsync(_endpointId, new PublishBulkRequestModel {
                    NodesToAdd = _cache
                        .Select(n => new PublishedItemModel {
                            DisplayName = n.DisplayName.Text,
                            SamplingInterval = n.MinimumSamplingInterval == null ? (TimeSpan?)null :
                                TimeSpan.FromMilliseconds(n.MinimumSamplingInterval.Value),
                            NodeId = n.NodeId.AsString(ctx)
                        })
                        .ToList()
                });
                _cache.Clear();
            }

            private const int kMaxCacheSize = 1000;
            private readonly List<VariableNodeModel> _cache = new List<VariableNodeModel>();
            private readonly string _endpointId;
            private readonly string _sessionId;
            private readonly BulkPublishHandler _outer;
        }

        private readonly INodeSetProcessor _processor;
        private readonly IPublishServices _publish;
    }
}