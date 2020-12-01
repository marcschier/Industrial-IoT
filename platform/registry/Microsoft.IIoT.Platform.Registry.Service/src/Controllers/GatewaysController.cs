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
    /// Read, Update and Query Gateway resources
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/gateways")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class GatewaysController : ControllerBase {

        /// <summary>
        /// Create controller for Gateway services
        /// </summary>
        /// <param name="gateways"></param>
        public GatewaysController(IGatewayRegistry gateways) {
            _gateways = gateways;
        }

        /// <summary>
        /// Get Gateway registration information
        /// </summary>
        /// <remarks>
        /// Returns a Gateway's registration and connectivity information.
        /// A Gateway id corresponds to the twin modules module identity.
        /// </remarks>
        /// <param name="GatewayId">Gateway identifier</param>
        /// <returns>Gateway registration</returns>
        [HttpGet("{GatewayId}")]
        public async Task<GatewayInfoApiModel> GetGatewayAsync(string GatewayId) {
            var result = await _gateways.GetGatewayAsync(GatewayId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update Gateway configuration
        /// </summary>
        /// <remarks>
        /// Allows a caller to configure operations on the Gateway module
        /// identified by the Gateway id.
        /// </remarks>
        /// <param name="GatewayId">Gateway identifier</param>
        /// <param name="request">Patch request</param>
        [HttpPatch("{GatewayId}")]
        [Authorize(Policy = Policies.CanChange)]
        public async Task UpdateGatewayAsync(string GatewayId,
            [FromBody] [Required] GatewayUpdateApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _gateways.UpdateGatewayAsync(GatewayId,
                request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Get list of sites
        /// </summary>
        /// <remarks>
        /// List all sites gateways are registered in.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation
        /// token</param>
        /// <param name="pageSize">Optional number of results to
        /// return</param>
        /// <returns>Sites</returns>
        [HttpGet("sites")]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<GatewaySiteListApiModel> GetListOfSitesAsync(
            [FromQuery] string continuationToken, [FromQuery] int? pageSize) {

            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _gateways.ListSitesAsync(
                continuationToken, pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get list of Gateways
        /// </summary>
        /// <remarks>
        /// Get all registered Gateways and therefore twin modules in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>
        /// List of Gateways and continuation token to use for next request
        /// in x-ms-continuation header.
        /// </returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<GatewayListApiModel> GetListOfGatewayAsync(
            [FromQuery] string continuationToken,
            [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _gateways.ListGatewaysAsync(
                continuationToken, pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Query Gateways
        /// </summary>
        /// <remarks>
        /// Get all Gateways that match a specified query.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfGateway operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Gateway query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Gateway</returns>
        [HttpPost("query")]
        public async Task<GatewayListApiModel> QueryGatewayAsync(
            [FromBody] [Required] GatewayQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _gateways.QueryGatewaysAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of Gateways
        /// </summary>
        /// <remarks>
        /// Get a list of Gateways filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfGateway operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Gateway Query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Gateway</returns>
        [HttpGet("query")]
        public async Task<GatewayListApiModel> GetFilteredListOfGatewayAsync(
            [FromQuery] [Required] GatewayQueryApiModel query,
            [FromQuery] int? pageSize) {

            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _gateways.QueryGatewaysAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        private readonly IGatewayRegistry _gateways;
    }
}
