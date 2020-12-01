// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Twin.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Persistent twin twin registry
    /// </summary>
    public interface ITwinRegistry {

        /// <summary>
        /// Activate a new twin for communication
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinActivationResultModel> ActivateTwinAsync(
            TwinActivationRequestModel request, 
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get all twins in paged form
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoListModel> ListTwinsAsync(string continuation,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Find registration of the supplied twin.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoListModel> QueryTwinsAsync(
            TwinInfoQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get twin by identifer.
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinModel> GetTwinAsync(string twinId,
            CancellationToken ct = default);

        /// <summary>
        /// Update the twin
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateTwinAsync(string twinId, TwinInfoUpdateModel model,
            OperationContextModel context = null, 
            CancellationToken ct = default);

        /// <summary>
        /// Deactivate a twin for communication
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="generationId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <param name="ct"></param>
        Task DeactivateTwinAsync(string twinId, string generationId,
            OperationContextModel context = null,
            CancellationToken ct = default);
    }
}
