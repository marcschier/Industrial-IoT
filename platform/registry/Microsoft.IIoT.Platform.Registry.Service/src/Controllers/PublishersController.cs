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
    /// Read, Update and Query publisher resources
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/publishers")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanQuery)]
    [ApiController]
    public class PublishersController : ControllerBase {

        /// <summary>
        /// Create controller for publisher services
        /// </summary>
        /// <param name="publishers"></param>
        public PublishersController(IPublisherRegistry publishers) {
            _publishers = publishers;
        }

        /// <summary>
        /// Get publisher registration information
        /// </summary>
        /// <remarks>
        /// Returns a publisher's registration and connectivity information.
        /// A publisher id corresponds to the twin modules module identity.
        /// </remarks>
        /// <param name="publisherId">Publisher identifier</param>
        /// <returns>Publisher registration</returns>
        [HttpGet("{publisherId}")]
        public async Task<PublisherApiModel> GetPublisherAsync(string publisherId) {
            var result = await _publishers.GetPublisherAsync(
                publisherId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update publisher configuration
        /// </summary>
        /// <remarks>
        /// Allows a caller to configure operations on the publisher module
        /// identified by the publisher id.
        /// </remarks>
        /// <param name="publisherId">Publisher identifier</param>
        /// <param name="request">Patch request</param>
        /// <returns></returns>
        [HttpPatch("{publisherId}")]
        [Authorize(Policy = Policies.CanChange)]
        public async Task UpdatePublisherAsync(string publisherId,
            [FromBody][Required] PublisherUpdateApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _publishers.UpdatePublisherAsync(publisherId,
                request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Get list of publishers
        /// </summary>
        /// <remarks>
        /// Get all registered publishers and therefore twin modules in paged form.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call this operation again using the token to retrieve more results.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>
        /// List of publishers and continuation token to use for next request
        /// in x-ms-continuation header.
        /// </returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<PublisherListApiModel> GetListOfPublisherAsync(
            [FromQuery] string continuationToken,
            [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _publishers.ListPublishersAsync(continuationToken,
                pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Query publishers
        /// </summary>
        /// <remarks>
        /// Get all publishers that match a specified query.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfPublisher operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Publisher query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Publisher</returns>
        [HttpPost("query")]
        public async Task<PublisherListApiModel> QueryPublisherAsync(
            [FromBody][Required] PublisherQueryApiModel query,
            [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _publishers.QueryPublishersAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            // TODO: Filter results based on RBAC

            return result.ToApiModel();
        }

        /// <summary>
        /// Get filtered list of publishers
        /// </summary>
        /// <remarks>
        /// Get a list of publishers filtered using the specified query parameters.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfPublisher operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Publisher Query model</param>
        /// <param name="pageSize">Number of results to return</param>
        /// <returns>Publisher</returns>
        [HttpGet("query")]
        public async Task<PublisherListApiModel> GetFilteredListOfPublisherAsync(
            [FromQuery][Required] PublisherQueryApiModel query,
            [FromQuery] int? pageSize) {

            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _publishers.QueryPublishersAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            // TODO: Filter results based on RBAC

            return result.ToApiModel();
        }

        private readonly IPublisherRegistry _publishers;
    }
}
