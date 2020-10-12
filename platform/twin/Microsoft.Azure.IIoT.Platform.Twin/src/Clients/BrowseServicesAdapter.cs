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
    public sealed class BrowseServicesAdapter : IBrowseServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="browse"></param>
        public BrowseServicesAdapter(IEndpointRegistry registry,
            IBrowseServices<EndpointModel> browse) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _browse = browse ?? throw new ArgumentNullException(nameof(browse));
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            string endpoint, BrowseRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _browse.NodeBrowseFirstAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            string endpoint, BrowseNextRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _browse.NodeBrowseNextAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            string endpoint, BrowsePathRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _browse.NodeBrowsePathAsync(ep, request).ConfigureAwait(false);
        }

        private readonly IEndpointRegistry _registry;
        private readonly IBrowseServices<EndpointModel> _browse;
    }
}
