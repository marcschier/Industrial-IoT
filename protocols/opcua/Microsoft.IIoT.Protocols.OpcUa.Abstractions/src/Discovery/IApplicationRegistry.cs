// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Application registry
    /// </summary>
    public interface IApplicationRegistry {

        /// <summary>
        /// Register application using the specified information.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationRegistrationResultModel> RegisterApplicationAsync(
            ApplicationRegistrationRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Read full application model for specified
        /// application (server/client) which includes all
        /// endpoints if there are any.
        /// </summary>
        /// <param name="applicationId">The applicationId</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationRegistrationModel> GetApplicationAsync(
            string applicationId, CancellationToken ct = default);

        /// <summary>
        /// Update an existing application, e.g. server
        /// certificate, or additional capabilities.
        /// </summary>
        /// <param name="applicationId">The applicationId</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// List all applications or continue find query.
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoListModel> ListApplicationsAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find applications for the specified information
        /// criterias.  The returned continuation if any must
        /// be passed to ListApplicationsAsync.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoListModel> QueryApplicationsAsync(
            ApplicationInfoQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Unregister application and all associated endpoints.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="generationId"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnregisterApplicationAsync(string applicationId,
            string generationId, OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Clean up all applications and endpoints that have not
        /// been seen since for the amount of time
        /// </summary>
        /// <param name="notSeenFor"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PurgeLostApplicationsAsync(TimeSpan notSeenFor,
            OperationContextModel context = null,
            CancellationToken ct = default);
    }
}
