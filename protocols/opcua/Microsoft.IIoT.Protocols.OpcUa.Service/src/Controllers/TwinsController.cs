// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers {
    using Microsoft.IIoT.Protocols.OpcUa.Service.Filters;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Extensions.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Create and remove twins
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/twins")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public class TwinsController : ControllerBase {

        /// <summary>
        /// Create controller for twins services
        /// </summary>
        /// <param name="twins"></param>
        public TwinsController(ITwinRegistry twins) {
            _twins = twins;
        }

        /// <summary>
        /// Get twin information
        /// </summary>
        /// <remarks>
        /// Gets information about an twin.
        /// </remarks>
        /// <param name="request">twin activation request</param>
        /// <returns>Twin registration</returns>
        [HttpPut]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<TwinActivationResponseApiModel> ActivateTwinAsync(
            [FromBody] TwinActivationRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            // TODO: add operation context
            var result = await _twins.ActivateTwinAsync(
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get twin information
        /// </summary>
        /// <remarks>
        /// Gets information about an twin.
        /// </remarks>
        /// <param name="twinId">twin identifier</param>
        /// <returns>Twin registration</returns>
        [HttpGet("{twinId}")]
        public async Task<TwinApiModel> GetTwinAsync(string twinId) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var result = await _twins.GetTwinAsync(twinId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get list of twins
        /// </summary>
        /// <remarks>
        /// Get all registered twins in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>List of twins and continuation token to use for next request</returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<TwinInfoListApiModel> GetListOfTwinsAsync(
            [FromQuery] string continuationToken,
            [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _twins.ListTwinsAsync(continuationToken,
                pageSize).ConfigureAwait(false);

            // TODO: Redact username/token based on policy/permission

            return result.ToApiModel();
        }

        /// <summary>
        /// Query twins
        /// </summary>
        /// <remarks>
        /// Return twins that match the specified query.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfTwins operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Query to match</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>List of twins and continuation token to use for next request</returns>
        [HttpPost("query")]
        public async Task<TwinInfoListApiModel> QueryTwinsAsync(
            [FromBody][Required] TwinInfoQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _twins.QueryTwinsAsync(query.ToServiceModel(),
                pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of twins
        /// </summary>
        /// <remarks>
        /// Get a list of twins filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfTwins operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Query to match</param>
        /// <param name="pageSize">Optional number of results to
        /// return</param>
        /// <returns>List of twins and continuation token to use for next request</returns>
        [HttpGet("query")]
        public async Task<TwinInfoListApiModel> GetFilteredListOfTwinsAsync(
            [FromQuery][Required] TwinInfoQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _twins.QueryTwinsAsync(query.ToServiceModel(),
                pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Deactivate twin
        /// </summary>
        /// <remarks>
        /// Removes a twin with the specified generation identifier.
        /// If resource is out of date error is returned patching should be
        /// retried with the current generation.
        /// </remarks>
        /// <param name="twinId">The wtwin identifier</param>
        /// <param name="generationId">Twin generation</param>
        /// <returns></returns>
        [HttpDelete("{twinId}/{generationId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task GroupAsync(string twinId, string generationId) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _twins.DeactivateTwinAsync(twinId, generationId,
                new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _twins;
    }
}
