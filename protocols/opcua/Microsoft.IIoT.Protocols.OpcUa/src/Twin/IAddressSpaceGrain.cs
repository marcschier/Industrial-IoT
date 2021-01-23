// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin grain
    /// </summary>
    public interface IAddressSpaceGrain : IGrainWithStringKey {

        /// <summary>
        /// Get certificate
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<X509CertificateChainModel> GetCertificateAsync(
            CancellationToken ct);

        /// <summary>
        /// Read historic values
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            HistoryReadRequestModel<VariantValue> request,
            CancellationToken ct);

        /// <summary>
        /// Continue reading history values
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            HistoryReadNextRequestModel request, CancellationToken ct);

        /// <summary>
        /// Update historic values
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            HistoryUpdateRequestModel<VariantValue> request,
            CancellationToken ct);

        /// <summary>
        /// Browse first
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseResultModel> NodeBrowseFirstAsync(
            BrowseRequestModel request, CancellationToken ct);

        /// <summary>
        /// Browse next 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseNextResultModel> NodeBrowseNextAsync(
            BrowseNextRequestModel request, CancellationToken ct);

        /// <summary>
        /// Browse path
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowsePathResultModel> NodeBrowsePathAsync(
            BrowsePathRequestModel request, CancellationToken ct);

        /// <summary>
        /// Call method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodCallResultModel> NodeMethodCallAsync(
            MethodCallRequestModel request, CancellationToken ct);

        /// <summary>
        /// Get method meta data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            MethodMetadataRequestModel request, CancellationToken ct);

        /// <summary>
        /// Read node
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ReadResultModel> NodeReadAsync(ReadRequestModel request,
            CancellationToken ct);

        /// <summary>
        /// Write node
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriteResultModel> NodeWriteAsync(
            WriteRequestModel request, CancellationToken ct);

        /// <summary>
        /// Read value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueReadResultModel> NodeValueReadAsync(
            ValueReadRequestModel request, CancellationToken ct);

        /// <summary>
        /// Write value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueWriteResultModel> NodeValueWriteAsync(
            ValueWriteRequestModel request, CancellationToken ct);
    }
}