// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Twin.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Browse nodes services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/browse")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanBrowse)]
    [ApiController]
    public class BrowseController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="browser"></param>
        public BrowseController(IBrowseServices<string> browser) {
            _browser = browser;
        }

        /// <summary>
        /// Browse node references
        /// </summary>
        /// <remarks>
        /// Browse a node on the specified twin.
        /// The twin must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated twin.</param>
        /// <param name="request">The browse request</param>
        /// <returns>The browse response</returns>
        [HttpPost("{twinId}")]
        public async Task<BrowseResponseApiModel> BrowseAsync(string twinId,
            [FromBody] [Required] BrowseRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var browseresult = await _browser.NodeBrowseAsync(twinId,
                request.ToServiceModel()).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        /// <summary>
        /// Browse next set of references
        /// </summary>
        /// <remarks>
        /// Browse next set of references on the twin.
        /// The twin must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated twin.</param>
        /// <param name="request">The request body with continuation token.</param>
        /// <returns>The browse response</returns>
        [HttpPost("{twinId}/next")]
        public async Task<BrowseNextResponseApiModel> BrowseNextAsync(
            string twinId, [FromBody] [Required] BrowseNextRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.ContinuationToken == null) {
                throw new ArgumentException("Missing continuation", nameof(request));
            }
            var browseresult = await _browser.NodeBrowseNextAsync(twinId,
                request.ToServiceModel()).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        /// <summary>
        /// Browse using a browse path
        /// </summary>
        /// <remarks>
        /// Browse using a path from the specified node id.
        /// This call uses TranslateBrowsePathsToNodeIds service under the hood.
        /// The twin must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated twin.</param>
        /// <param name="request">The browse path request</param>
        /// <returns>The browse path response</returns>
        [HttpPost("{twinId}/path")]
        public async Task<BrowsePathResponseApiModel> BrowseUsingPathAsync(string twinId,
            [FromBody] [Required] BrowsePathRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var browseresult = await _browser.NodeBrowsePathAsync(twinId,
                request.ToServiceModel()).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        /// <summary>
        /// Browse set of unique target nodes
        /// </summary>
        /// <remarks>
        /// Browse the set of unique hierarchically referenced target nodes on the twin.
        /// The twin must be activated and connected and the module client
        /// and server must trust each other.
        /// The root node id to browse from can be provided as part of the query
        /// parameters.
        /// If it is not provided, the RootFolder node is browsed. Note that this
        /// is the same as the POST method with the model containing the node id
        /// and the targetNodesOnly flag set to true.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated twin.</param>
        /// <param name="nodeId">The node to browse or omit to browse the root node (i=84)
        /// </param>
        /// <returns>The browse response</returns>
        [HttpGet("{twinId}")]
        public async Task<BrowseResponseApiModel> GetSetOfUniqueNodesAsync(
            string twinId, [FromQuery] string nodeId) {
            if (string.IsNullOrEmpty(nodeId)) {
                nodeId = null;
            }
            var request = new BrowseRequestModel {
                NodeId = nodeId,
                TargetNodesOnly = true,
                ReadVariableValues = true
            };
            var browseresult = await _browser.NodeBrowseAsync(twinId, 
                request).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        /// <summary>
        /// Browse next set of unique target nodes
        /// </summary>
        /// <remarks>
        /// Browse the next set of unique hierarchically referenced target 
        /// nodes on the twin.
        /// The twin must be activated and connected and the module client
        /// and server must trust each other.
        /// Note that this is the same as the POST method with the model containing
        /// the continuation token and the targetNodesOnly flag set to true.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated twin.</param>
        /// <param name="continuationToken">Continuation token from 
        /// GetSetOfUniqueNodes operation</param>
        /// <returns>The browse response</returns>
        [HttpGet("{twinId}/next")]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<BrowseNextResponseApiModel> GetNextSetOfUniqueNodesAsync(
            string twinId, [FromQuery] [Required] string continuationToken) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            if (string.IsNullOrEmpty(continuationToken)) {
                throw new ArgumentNullException(nameof(continuationToken));
            }
            var request = new BrowseNextRequestModel {
                ContinuationToken = continuationToken,
                TargetNodesOnly = true,
                ReadVariableValues = true
            };
            var browseresult = await _browser.NodeBrowseNextAsync(twinId, 
                request).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        private readonly IBrowseServices<string> _browser;
    }
}
