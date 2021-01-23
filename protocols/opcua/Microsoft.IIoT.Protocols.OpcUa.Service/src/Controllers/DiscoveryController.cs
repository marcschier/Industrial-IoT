// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers {
    using Microsoft.IIoT.Protocols.OpcUa.Service.Filters;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Rpc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Configure discovery
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/discovery")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class DiscoveryController : ControllerBase {

        /// <summary>
        /// Create controller for discovery services
        /// </summary>
        /// <param name="discovery"></param>
        /// <param name="events"></param>
        public DiscoveryController(IDiscoveryServices discovery,
            IGroupRegistrationT<DiscoveryHub> events) {
            _events = events;
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
            [FromBody][Required] ServerRegistrationRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = (OperationContextModel)null;
            // TODO: var context.AuthorityId = User.Identity.Name;
            await _discovery.RegisterAsync(request.ToServiceModel(),
                context).ConfigureAwait(false);
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
        [HttpPost("requests")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task DiscoverServerAsync(
            [FromBody][Required] DiscoveryRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = (OperationContextModel)null;
            // TODO: var context.AuthorityId = User.Identity.Name;
            await _discovery.DiscoverAsync(request.ToServiceModel(),
                context).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancel discovery
        /// </summary>
        /// <remarks>
        /// Cancels a discovery request using the request identifier.
        /// </remarks>
        /// <param name="requestId">Discovery request</param>
        /// <returns></returns>
        [HttpDelete("requests/{requestId}")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task CancelAsync(string requestId) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            var context = (OperationContextModel)null;
            await _discovery.CancelAsync(new DiscoveryCancelModel {
                Id = requestId
            }, context).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to discovery progress for a request
        /// </summary>
        /// <remarks>
        /// Register a client to receive discovery progress events
        /// through SignalR for a particular request.
        /// </remarks>
        /// <param name="requestId">The request to monitor</param>
        /// <param name="connectionId">The connection that will receive discovery
        /// events.</param>
        /// <returns></returns>
        [HttpPut("requests/{requestId}/events")]
        public async Task SubscribeByRequestIdAsync(string requestId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(requestId, connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from discovery progress for a request.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving discovery
        /// events for a particular request.
        /// </remarks>
        /// <param name="requestId">The request to unsubscribe from
        /// </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more discovery progress</param>
        /// <returns></returns>
        [HttpDelete("requests/{requestId}/events/{connectionId}")]
        public async Task UnsubscribeByRequestIdAsync(string requestId,
            string connectionId) {
            await _events.UnsubscribeAsync(requestId, connectionId).ConfigureAwait(false);
        }




        // ------------------- todo - remove



        /// <summary>
        /// Subscribe to discovery progress from discoverer
        /// </summary>
        /// <remarks>
        /// Register a client to receive discovery progress events
        /// through SignalR from a particular discoverer.
        /// </remarks>
        /// <param name="discovererId">The discoverer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive discovery
        /// events.</param>
        /// <returns></returns>
        [HttpPut("{discovererId}/events")]
        public async Task SubscribeByDiscovererIdAsync(string discovererId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(discovererId, connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from discovery progress from discoverer.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving discovery events.
        /// </remarks>
        /// <param name="discovererId">The discoverer to unsubscribe from
        /// </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more discovery progress</param>
        /// <returns></returns>
        [HttpDelete("{discovererId}/events/{connectionId}")]
        public async Task UnsubscribeByDiscovererIdAsync(string discovererId,
            string connectionId) {
            await _events.UnsubscribeAsync(discovererId, connectionId).ConfigureAwait(false);
        }

        private readonly IGroupRegistrationT<DiscoveryHub> _events;
        private readonly IDiscoveryServices _discovery;
    }
}
