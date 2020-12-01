// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api {
    using Microsoft.IIoT.Platform.Twin.Api.Models;
    using Microsoft.IIoT.Serializers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents OPC twinId service api functions
    /// </summary>
    public interface ITwinServiceApi {

        /// <summary>
        /// Returns status of the service
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string> GetServiceStatusAsync(CancellationToken ct = default);

        /// <summary>
        /// Activate a new twinId for communication
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinActivationResponseApiModel> ActivateTwinAsync(
            TwinActivationRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Get all twins in paged form
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoListApiModel> ListTwinsAsync(string continuation,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Find registration of the supplied twinId.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoListApiModel> QueryTwinsAsync(
            TwinInfoQueryApiModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get twinId by identifer.
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinApiModel> GetTwinAsync(string twinId,
            CancellationToken ct = default);

        /// <summary>
        /// Update the twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="model"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateTwinAsync(string twinId,
            TwinInfoUpdateApiModel model, CancellationToken ct = default);

        /// <summary>
        /// Browse node on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseResponseApiModel> NodeBrowseFirstAsync(string twinId,
            BrowseRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Browse next references on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseNextResponseApiModel> NodeBrowseNextAsync(string twinId,
            BrowseNextRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Browse by path on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowsePathResponseApiModel> NodeBrowsePathAsync(string twinId,
            BrowsePathRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Call method on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodCallResponseApiModel> NodeMethodCallAsync(string twinId,
            MethodCallRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Get meta data for method call on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodMetadataResponseApiModel> NodeMethodGetMetadataAsync(string twinId,
            MethodMetadataRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Read node value on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueReadResponseApiModel> NodeValueReadAsync(string twinId,
            ValueReadRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Write node value on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueWriteResponseApiModel> NodeValueWriteAsync(string twinId,
            ValueWriteRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Read node attributes on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ReadResponseApiModel> NodeReadAsync(string twinId,
            ReadRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Write node attributes on twinId
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriteResponseApiModel> NodeWriteAsync(string twinId,
            WriteRequestApiModel request, CancellationToken ct = default);

        /// <summary>
        /// Read node history with custom encoded extension object details
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadRawAsync(
            string endpointId, HistoryReadRequestApiModel<VariantValue> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read history call with custom encoded extension object details
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadRawNextAsync(
            string endpointId, HistoryReadNextRequestApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Update using raw extension object details
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResponseApiModel> HistoryUpdateRawAsync(
            string endpointId, HistoryUpdateRequestApiModel<VariantValue> request,
            CancellationToken ct = default);

        /// <summary>
        /// Start model upload
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="content"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModelUploadStartResponseApiModel> ModelUploadStartAsync(string twinId,
            ModelUploadStartRequestApiModel content, CancellationToken ct = default);

        /// <summary>
        /// Deactivate a twinId for communication
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DectivateTwinAsync(string twinId, string generationId, 
            CancellationToken ct = default);
    }

}
