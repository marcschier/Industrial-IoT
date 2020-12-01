// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Service.Controllers {
    using Microsoft.IIoT.Platform.Registry.Service.Auth;
    using Microsoft.IIoT.Platform.Registry.Service.Filters;
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using Microsoft.IIoT.Platform.Registry;
    using Microsoft.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using System;

    /// <summary>
    /// Read, Update and Query supervisor resources
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/supervisors")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class SupervisorsController : ControllerBase {

        /// <summary>
        /// Create controller for supervisor services
        /// </summary>
        /// <param name="supervisors"></param>
        public SupervisorsController(ISupervisorRegistry supervisors) {
            _supervisors = supervisors;
        }

        /// <summary>
        /// Get supervisor registration information
        /// </summary>
        /// <remarks>
        /// Returns a supervisor's registration and connectivity information.
        /// A supervisor id corresponds to the twin modules module identity.
        /// </remarks>
        /// <param name="supervisorId">Supervisor identifier</param>
        /// <returns>Supervisor registration</returns>
        [HttpGet("{supervisorId}")]
        public async Task<SupervisorApiModel> GetSupervisorAsync(string supervisorId) {
            var result = await _supervisors.GetSupervisorAsync(
                supervisorId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update supervisor information
        /// </summary>
        /// <remarks>
        /// Allows a caller to configure recurring discovery runs on the twin module
        /// identified by the supervisor id or update site information.
        /// </remarks>
        /// <param name="supervisorId">supervisor identifier</param>
        /// <param name="request">Patch request</param>
        /// <returns></returns>
        [HttpPatch("{supervisorId}")]
        [Authorize(Policy = Policies.CanChange)]
        public async Task UpdateSupervisorAsync(string supervisorId,
            [FromBody][Required] SupervisorUpdateApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _supervisors.UpdateSupervisorAsync(supervisorId,
                request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Get list of supervisors
        /// </summary>
        /// <remarks>
        /// Get all registered supervisors and therefore twin modules in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>
        /// List of supervisors and continuation token to use for next request
        /// in x-ms-continuation header.
        /// </returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<SupervisorListApiModel> GetListOfSupervisorsAsync(
            [FromQuery] string continuationToken, [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _supervisors.ListSupervisorsAsync(
                continuationToken, pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Query supervisors
        /// </summary>
        /// <remarks>
        /// Get all supervisors that match a specified query.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfSupervisors operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Supervisors query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Supervisors</returns>
        [HttpPost("query")]
        public async Task<SupervisorListApiModel> QuerySupervisorsAsync(
            [FromBody][Required] SupervisorQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _supervisors.QuerySupervisorsAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            // TODO: Filter results based on RBAC

            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of supervisors
        /// </summary>
        /// <remarks>
        /// Get a list of supervisors filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfSupervisors operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Supervisors Query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Supervisors</returns>
        [HttpGet("query")]
        public async Task<SupervisorListApiModel> GetFilteredListOfSupervisorsAsync(
            [FromQuery][Required] SupervisorQueryApiModel query,
            [FromQuery] int? pageSize) {

            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _supervisors.QuerySupervisorsAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            // TODO: Filter results based on RBAC

            return result.ToApiModel();
        }

        private readonly ISupervisorRegistry _supervisors;
    }
}
