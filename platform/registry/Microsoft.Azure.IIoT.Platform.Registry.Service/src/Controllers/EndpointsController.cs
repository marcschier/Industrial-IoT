// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Registry.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Registry.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Activate, Deactivate and Query endpoint resources
    /// </summary>
    [ApiVersion("2")][ApiVersion("3")]
    [Route("v{version:apiVersion}/endpoints")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class EndpointsController : ControllerBase {

        /// <summary>
        /// Create controller for endpoints services
        /// </summary>
        /// <param name="endpoints"></param>
        public EndpointsController(IEndpointRegistry endpoints) {
            _endpoints = endpoints;
        }

        /// <summary>
        /// Get endpoint certificate chain
        /// </summary>
        /// <remarks>
        /// Gets current certificate of the endpoint.
        /// </remarks>
        /// <param name="endpointId">endpoint identifier</param>
        /// <returns>Endpoint registration</returns>
        [HttpGet("{endpointId}/certificate")]
        public async Task<X509CertificateChainApiModel> GetEndpointCertificateAsync(
            string endpointId) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var result = await _endpoints.GetEndpointCertificateAsync(endpointId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update endpoint information
        /// </summary>
        /// <remarks>
        /// The endpoint information is updated with new properties.  Note that
        /// this information might be overridden if the endpoint is re-discovered
        /// during a discovery run (recurring or one-time).
        /// </remarks>
        /// <param name="endpointId">The identifier of the endpoint</param>
        /// <param name="request">Endpoint update request</param>
        [HttpPatch("{endpointId}")]
        [Authorize(Policy = Policies.CanChange)]
        public async Task UpdateEndpointAsync(string endpointId,
            [FromBody][Required] EndpointInfoUpdateApiModel request) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var model = request.ToServiceModel();
            // TODO: model.AuthorityId = User.Identity.Name;
            await _endpoints.UpdateEndpointAsync(endpointId, model).ConfigureAwait(false);
        }

        /// <summary>
        /// Get endpoint information
        /// </summary>
        /// <remarks>
        /// Gets information about an endpoint.
        /// </remarks>
        /// <param name="endpointId">endpoint identifier</param>
        /// <returns>Endpoint registration</returns>
        [HttpGet("{endpointId}")]
        public async Task<EndpointInfoApiModel> GetEndpointAsync(string endpointId) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var result = await _endpoints.GetEndpointAsync(endpointId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get list of endpoints
        /// </summary>
        /// <remarks>
        /// Get all registered endpoints in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>List of endpoints and continuation token to use for next request</returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<EndpointInfoListApiModel> GetListOfEndpointsAsync(
            [FromQuery] string continuationToken,
            [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _endpoints.ListEndpointsAsync(continuationToken,
                pageSize).ConfigureAwait(false);

            // TODO: Redact username/token based on policy/permission

            return result.ToApiModel();
        }

        /// <summary>
        /// Query endpoints
        /// </summary>
        /// <remarks>
        /// Return endpoints that match the specified query.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfEndpoints operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Query to match</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>List of endpoints and continuation token to use for next request</returns>
        [HttpPost("query")]
        public async Task<EndpointInfoListApiModel> QueryEndpointsAsync(
            [FromBody] [Required] EndpointInfoQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _endpoints.QueryEndpointsAsync(query.ToServiceModel(),
                pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of endpoints
        /// </summary>
        /// <remarks>
        /// Get a list of endpoints filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfEndpoints operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Query to match</param>
        /// <param name="pageSize">Optional number of results to
        /// return</param>
        /// <returns>List of endpoints and continuation token to use for next request</returns>
        [HttpGet("query")]
        public async Task<EndpointInfoListApiModel> GetFilteredListOfEndpointsAsync(
            [FromQuery] [Required] EndpointInfoQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _endpoints.QueryEndpointsAsync(query.ToServiceModel(),
                pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        private readonly IEndpointRegistry _endpoints;
    }
}
