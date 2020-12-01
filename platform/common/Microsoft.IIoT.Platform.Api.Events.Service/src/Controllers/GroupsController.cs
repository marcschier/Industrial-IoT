// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Api.Events.Service.Controllers {
    using Microsoft.IIoT.Platform.Api.Events.Service.Auth;
    using Microsoft.IIoT.Platform.Api.Events.Service.Filters;
    using Microsoft.IIoT.Rpc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset message monitoring services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/groups")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class GroupsController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="events"></param>
        public GroupsController(IGroupRegistrationT<GroupsHub> events) {
            _events = events;
        }

        /// <summary>
        /// Subscribe to receive dataset messages
        /// </summary>
        /// <remarks>
        /// Register a client to receive dataset messages through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive publisher
        /// samples.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/messages")]
        public async Task SubscribeAsync(string dataSetWriterId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(dataSetWriterId, connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset messages.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving messages.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to unsubscribe from
        /// </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more dataset messages</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/messages/{connectionId}")]
        public async Task UnsubscribeAsync(string dataSetWriterId, string connectionId) {
            await _events.UnsubscribeAsync(dataSetWriterId, connectionId).ConfigureAwait(false);
        }

        private readonly IGroupRegistrationT<GroupsHub> _events;
    }
}
