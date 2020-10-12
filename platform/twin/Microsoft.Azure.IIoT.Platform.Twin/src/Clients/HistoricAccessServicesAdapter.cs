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
    using Microsoft.Azure.IIoT.Serializers;

    /// <summary>
    /// Implements ha services as adapter through endpoint registry
    /// </summary>
    public sealed class HistoricAccessServicesAdapter : IHistoricAccessServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="history"></param>
        public HistoricAccessServicesAdapter(IEndpointRegistry registry,
            IHistoricAccessServices<EndpointModel> history) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            string endpoint, HistoryReadRequestModel<VariantValue> request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _history.HistoryReadAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            string endpoint, HistoryReadNextRequestModel request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _history.HistoryReadNextAsync(ep, request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            string endpoint, HistoryUpdateRequestModel<VariantValue> request) {
            var ep = await _registry.GetActivatedEndpointAsync(endpoint).ConfigureAwait(false);
            return await _history.HistoryUpdateAsync(ep, request).ConfigureAwait(false);
        }

        private readonly IEndpointRegistry _registry;
        private readonly IHistoricAccessServices<EndpointModel> _history;
    }
}
