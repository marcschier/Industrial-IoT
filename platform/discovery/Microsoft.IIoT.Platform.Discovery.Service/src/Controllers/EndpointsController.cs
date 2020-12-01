// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Service.Controllers {
    using Microsoft.IIoT.Platform.Discovery.Service.Filters;
    using Microsoft.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Activate, Deactivate and Query endpoint resources
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/endpoints")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class EndpointsController : ControllerBase {

        /// <summary>
        /// Create controller for endpoints services
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="certificates"></param>
        public EndpointsController(IEndpointRegistry endpoints,
            ICertificateServices<string> certificates) {
            _endpoints = endpoints;
            _certificates = certificates;
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
            var result = await _certificates.GetCertificateAsync(
                endpointId).ConfigureAwait(false);
            return result.ToApiModel();
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
            [FromBody][Required] EndpointInfoQueryApiModel query,
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
            [FromQuery][Required] EndpointInfoQueryApiModel query,
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
        private readonly ICertificateServices<string> _certificates;
    }
}
