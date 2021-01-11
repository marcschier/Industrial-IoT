// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Historic access services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHistoricAccessServices<T> {

        /// <summary>
        /// Read node history
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(T twin,
            HistoryReadRequestModel<VariantValue> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read node history continuation
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(T twin,
            HistoryReadNextRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Update node history
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryUpdateAsync(T twin,
            HistoryUpdateRequestModel<VariantValue> request,
            CancellationToken ct = default);
    }
}
