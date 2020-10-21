// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements ha services as adapter through twin registry
    /// </summary>
    public sealed class HistoricAccessServicesAdapter : IHistoricAccessServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="history"></param>
        public HistoricAccessServicesAdapter(ITwinRegistry registry,
            IHistoricAccessServices<ConnectionModel> history) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            string twin, HistoryReadRequestModel<VariantValue> request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryReadAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryReadNextAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            string twin, HistoryUpdateRequestModel<VariantValue> request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryUpdateAsync(conn, request, ct).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _registry;
        private readonly IHistoricAccessServices<ConnectionModel> _history;
    }
}
