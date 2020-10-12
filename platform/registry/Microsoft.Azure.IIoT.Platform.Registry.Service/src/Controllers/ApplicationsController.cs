// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Registry.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Registry.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// CRUD and Query application resources
    /// </summary>
    [ApiVersion("2")][ApiVersion("3")]
    [Route("v{version:apiVersion}/applications")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class ApplicationsController : ControllerBase {

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="applications"></param>
        /// <param name="discovery"></param>
        public ApplicationsController(IApplicationRegistry applications,
            IDiscoveryServices discovery) {
            _applications = applications;
            _discovery = discovery;
        }

        /// <summary>
        /// Register new server
        /// </summary>
        /// <remarks>
        /// Registers a server solely using a discovery url. Requires that
        /// the onboarding agent service is running and the server can be
        /// located by a supervisor in its network using the discovery url.
        /// </remarks>
        /// <param name="request">Server registration request</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.CanManage)]
        public async Task RegisterServerAsync(
            [FromBody] [Required] ServerRegistrationRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _discovery.RegisterAsync(request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Discover servers
        /// </summary>
        /// <remarks>
        /// Registers servers by running a discovery scan in a supervisor's
        /// network. Requires that the onboarding agent service is running.
        /// </remarks>
        /// <param name="request">Discovery request</param>
        /// <returns></returns>
        [HttpPost("discover")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task DiscoverServerAsync(
            [FromBody] [Required] DiscoveryRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _discovery.DiscoverAsync(request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancel discovery
        /// </summary>
        /// <remarks>
        /// Cancels a discovery request using the request identifier.
        /// </remarks>
        /// <param name="requestId">Discovery request</param>
        /// <returns></returns>
        [HttpDelete("discover/{requestId}")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task CancelAsync(string requestId) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            await _discovery.CancelAsync(new DiscoveryCancelModel {
                Id = requestId
                // TODO: AuthorityId = User.Identity.Name;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Create new application
        /// </summary>
        /// <remarks>
        /// The application is registered using the provided information, but it
        /// is not associated with a supervisor.  This is useful for when you need
        /// to register clients or you want to register a server that is located
        /// in a network not reachable through a Twin module.
        /// </remarks>
        /// <param name="request">Application registration request</param>
        /// <returns>Application registration response</returns>
        [HttpPut]
        [Authorize(Policy = Policies.CanManage)]
        public async Task<ApplicationRegistrationResponseApiModel> CreateApplicationAsync(
            [FromBody] [Required] ApplicationRegistrationRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var model = request.ToServiceModel();
            // TODO: model.Context.AuthorityId = User.Identity.Name;
            var result = await _applications.RegisterApplicationAsync(model).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get application information
        /// </summary>
        /// <param name="applicationId">Application id for the server</param>
        /// <returns>Application registration</returns>
        [HttpGet("{applicationId}")]
        public async Task<ApplicationRegistrationApiModel> GetApplicationAsync(
            string applicationId) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            var result = await _applications.GetApplicationAsync(applicationId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update application information
        /// </summary>
        /// <remarks>
        /// The application information is updated with new properties.  Note that
        /// this information might be overridden if the application is re-discovered
        /// during a discovery run (recurring or one-time).
        /// </remarks>
        /// <param name="applicationId">The identifier of the application</param>
        /// <param name="request">Application update request</param>
        [HttpPatch("{applicationId}")]
        [Authorize(Policy = Policies.CanChange)]
        public async Task UpdateApplicationAsync(string applicationId,
            [FromBody] [Required] ApplicationInfoUpdateApiModel request) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var model = request.ToServiceModel();
            // TODO: model.Context.AuthorityId = User.Identity.Name;
            await _applications.UpdateApplicationAsync(applicationId, model).ConfigureAwait(false);
        }

        /// <summary>
        /// Unregister application
        /// </summary>
        /// <remarks>
        /// Unregisters and deletes application and all its associated endpoints.
        /// </remarks>
        /// <param name="applicationId">The identifier of the application</param>
        /// <param name="generationId">Generation id of the instance</param>
        /// <returns></returns>
        [HttpDelete("{applicationId}/{generationId}")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task DeleteApplicationAsync(string applicationId, string generationId) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            var context = (RegistryOperationContextModel)null;
            // TODO: context.AuthorityId = User.Identity.Name;
            await _applications.UnregisterApplicationAsync(applicationId, generationId,
                context).ConfigureAwait(false);
        }

        /// <summary>
        /// Purge applications
        /// </summary>
        /// <remarks>
        /// Purges all applications that have not been seen for a specified amount of time.
        /// </remarks>
        /// <param name="notSeenFor">A duration in milliseconds</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = Policies.CanManage)]
        public async Task DeleteAllDisabledApplicationsAsync(
            [FromQuery] TimeSpan? notSeenFor) {
            var context = (RegistryOperationContextModel)null;
            // TODO: context.AuthorityId = User.Identity.Name;
            await _applications.PurgeDisabledApplicationsAsync(
                notSeenFor ?? TimeSpan.FromTicks(0), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Get list of applications
        /// </summary>
        /// <remarks>
        /// Get all registered applications in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation
        /// token</param>
        /// <param name="pageSize">Optional number of results to
        /// return</param>
        /// <returns>
        /// List of servers and continuation token to use for next request
        /// in x-ms-continuation header.
        /// </returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<ApplicationInfoListApiModel> GetListOfApplicationsAsync(
            [FromQuery] string continuationToken, [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _applications.ListApplicationsAsync(
                continuationToken, pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Query applications
        /// </summary>
        /// <remarks>
        /// List applications that match a query model.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfApplications operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Application query</param>
        /// <param name="pageSize">Optional number of results to
        /// return</param>
        /// <returns>Applications</returns>
        [HttpPost("query")]
        public async Task<ApplicationInfoListApiModel> QueryApplicationsAsync(
            [FromBody] [Required] ApplicationRegistrationQueryApiModel query,
            [FromQuery] int? pageSize) {

            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _applications.QueryApplicationsAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of applications
        /// </summary>
        /// <remarks>
        /// Get a list of applications filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfApplications operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Applications Query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Applications</returns>
        [HttpGet("query")]
        public async Task<ApplicationInfoListApiModel> GetFilteredListOfApplicationsAsync(
            [FromBody] [Required] ApplicationRegistrationQueryApiModel query,
            [FromQuery] int? pageSize) {

            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _applications.QueryApplicationsAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        private readonly IApplicationRegistry _applications;
        private readonly IDiscoveryServices _discovery;
    }
}
