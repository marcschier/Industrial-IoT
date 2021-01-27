// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers {
    using Microsoft.IIoT.Protocols.OpcUa.Service.Filters;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.OpenApi;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Node read services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/nodes")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public class NodesController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="nodes"></param>
        /// <param name="history"></param>
        public NodesController(IBrowseServices<string> browser,
            INodeServices<string> nodes, IHistoricAccessServices<string> history) {
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
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
        [HttpPost("{twinId}/browse")]
        public async Task<BrowseResponseApiModel> BrowseAsync(string twinId,
            [FromBody][Required] BrowseRequestApiModel request) {
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
        [HttpPost("{twinId}/browse/next")]
        public async Task<BrowseNextResponseApiModel> BrowseNextAsync(
            string twinId, [FromBody][Required] BrowseNextRequestApiModel request) {
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
        [HttpPost("{twinId}/browse/path")]
        public async Task<BrowsePathResponseApiModel> BrowseUsingPathAsync(string twinId,
            [FromBody][Required] BrowsePathRequestApiModel request) {
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
        [HttpGet("{twinId}/browse")]
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
        [HttpGet("{twinId}/browse/next")]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<BrowseNextResponseApiModel> GetNextSetOfUniqueNodesAsync(
            string twinId, [FromQuery][Required] string continuationToken) {
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

        /// <summary>
        /// Read variable value
        /// </summary>
        /// <remarks>
        /// Get a variable node's value using its node id.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="nodeId">The node to read</param>
        /// <returns>The read value response</returns>
        [HttpGet("{twinId}/value")]
        public async Task<ValueReadResponseApiModel> GetValueAsync(
            string twinId, [FromQuery][Required] string nodeId) {
            if (string.IsNullOrEmpty(nodeId)) {
                throw new ArgumentNullException(nameof(nodeId));
            }
            var request = new ValueReadRequestApiModel { NodeId = nodeId };
            var readresult = await _nodes.NodeValueReadAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel();
        }

        /// <summary>
        /// Write variable value
        /// </summary>
        /// <remarks>
        /// Write variable node's value.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The write value request</param>
        /// <returns>The write value response</returns>
        [HttpPost("{twinId}/value")]
        public async Task<ValueWriteResponseApiModel> SetValueAsync(
            string twinId, [FromBody][Required] ValueWriteRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _nodes.NodeValueWriteAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Read variable value
        /// </summary>
        /// <remarks>
        /// Read a variable node's value.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The read value request</param>
        /// <returns>The read value response</returns>
        [HttpPost("{twinId}/value/read")]
        public async Task<ValueReadResponseApiModel> ReadValueAsync(
            string twinId, [FromBody][Required] ValueReadRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _nodes.NodeValueReadAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel();
        }

        /// <summary>
        /// Read node attributes
        /// </summary>
        /// <remarks>
        /// Read attributes of a node.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The read request</param>
        /// <returns>The read response</returns>
        [HttpPost("{twinId}/attributes/read")]
        public async Task<ReadResponseApiModel> ReadAttributesAsync(
            string twinId, [FromBody][Required] ReadRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _nodes.NodeReadAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel();
        }

        /// <summary>
        /// Write node attributes
        /// </summary>
        /// <remarks>
        /// Write any attribute of a node.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The batch write request</param>
        /// <returns>The batch write response</returns>
        [HttpPost("{twinId}/attributes/write")]
        public async Task<WriteResponseApiModel> WriteAttributesAsync(
            string twinId, [FromBody][Required] WriteRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _nodes.NodeWriteAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Get method meta data
        /// </summary>
        /// <remarks>
        /// Return method meta data to support a user interface displaying forms to
        /// input and output arguments.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The method metadata request</param>
        /// <returns>The method metadata response</returns>
        [HttpPost("{twinId}/call/$metadata")]
        public async Task<MethodMetadataResponseApiModel> GetCallMetadataAsync(
            string twinId, [FromBody][Required] MethodMetadataRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var metadataresult = await _nodes.NodeMethodGetMetadataAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return metadataresult.ToApiModel();
        }

        /// <summary>
        /// Call a method
        /// </summary>
        /// <remarks>
        /// Invoke method node with specified input arguments.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The method call request</param>
        /// <returns>The method call response</returns>
        [HttpPost("{twinId}/call")]
        public async Task<MethodCallResponseApiModel> CallMethodAsync(
            string twinId, [FromBody][Required] MethodCallRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            // TODO: Permissions

            var callresult = await _nodes.NodeMethodCallAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return callresult.ToApiModel();
        }

        /// <summary>
        /// Read history using json details
        /// </summary>
        /// <remarks>
        /// Read node history if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The history read response</returns>
        [HttpPost("{twinId}/history/read")]
        public async Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadRawAsync(
            string twinId, [FromBody][Required] HistoryReadRequestApiModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _history.HistoryReadAsync(
                twinId, request.ToServiceModel(d => d)).ConfigureAwait(false);
            return readresult.ToApiModel(d => d);
        }

        /// <summary>
        /// Read next batch of history as json
        /// </summary>
        /// <remarks>
        /// Read next batch of node history values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read next request</param>
        /// <returns>The history read response</returns>
        [HttpPost("{twinId}/history/read/next")]
        public async Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadRawNextAsync(
            string twinId, [FromBody][Required] HistoryReadNextRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _history.HistoryReadNextAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel(d => d);
        }

        /// <summary>
        /// Update node history using raw json
        /// </summary>
        /// <remarks>
        /// Update node history using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history update request</param>
        /// <returns>The history update result</returns>
        [HttpPost("{twinId}/history/update")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<HistoryUpdateResponseApiModel> HistoryUpdateRawAsync(
            string twinId, [FromBody][Required] HistoryUpdateRequestApiModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _history.HistoryUpdateAsync(
                twinId, request.ToServiceModel(d => d)).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }


        private readonly IHistoricAccessServices<string> _history;
        private readonly IBrowseServices<string> _browser;
        private readonly INodeServices<string> _nodes;
    }
}
