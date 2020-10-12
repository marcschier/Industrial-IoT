// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements node services as adapter through endpoint registry
    /// </summary>
    public sealed class NodeServicesAdapter : INodeServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="nodes"></param>
        public NodeServicesAdapter(IEndpointRegistry registry,
            INodeServices<EndpointModel> nodes) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            string endpoint, ValueReadRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeValueReadAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            string endpoint, ValueWriteRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeValueWriteAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            string endpoint, MethodMetadataRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeMethodGetMetadataAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            string endpoint, MethodCallRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeMethodCallAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            string endpoint, ReadRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeReadAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            string endpoint, WriteRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _nodes.NodeWriteAsync(ep, request).ConfigureAwait(false);
        }

        private readonly IEndpointRegistry _registry;
        private readonly INodeServices<EndpointModel> _nodes;
    }
}
