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
    using System;

    /// <summary>
    /// Dataset writer monitoring services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/writers")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class WritersController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="events"></param>
        public WritersController(IGroupRegistrationT<WritersHub> events) {
            _events = events;
        }

        /// <summary>
        /// Subscribe to receive dataset writer item status updates
        /// </summary>
        /// <remarks>
        /// Register a client to receive status updates for variables and events
        /// in the dataset through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive status
        /// updates.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/status")]
        public async Task SubscribeAsync(string dataSetWriterId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(dataSetWriterId,
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset writer item status updates.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving status updates
        /// for variables and events in the dataset.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more status updates.</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/status/{connectionId}")]
        public async Task UnsubscribeAsync(string dataSetWriterId,
            string connectionId) {
            await _events.UnsubscribeAsync(dataSetWriterId, 
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to receive samples
        /// </summary>
        /// <remarks>
        /// Register a client to receive publisher samples through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to subscribe to</param>
        /// <param name="variableId">The variable to subscribe to</param>
        /// <param name="connectionId">The connection that will receive publisher
        /// samples.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/variables/{variableId}")]
        public async Task SubscribeVariableAsync(string dataSetWriterId, string variableId,
            [FromBody] string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            await _events.SubscribeAsync($"{dataSetWriterId}_{variableId}", 
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving samples.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving samples.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="variableId">The variable to unsubscribe from</param>
        /// <param name="connectionId">The connection that will not receive
        /// any more published samples</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/variables/{variableId}/{connectionId}")]
        public async Task UnsubscribeVariableAsync(string dataSetWriterId, string variableId,
            string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            await _events.UnsubscribeAsync($"{dataSetWriterId}_{variableId}", 
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to receive dataset event messages
        /// </summary>
        /// <remarks>
        /// Register a client to receive publisher samples through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive publisher
        /// samples.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/event")]
        public async Task SubscribeEventsAsync(string dataSetWriterId, 
            [FromBody] string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _events.SubscribeAsync($"{dataSetWriterId}_event",
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset event messages.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving samples.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more published samples</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/event/{connectionId}")]
        public async Task UnsubscribeEventsAsync(string dataSetWriterId,
            string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _events.UnsubscribeAsync($"{dataSetWriterId}_event",
                connectionId).ConfigureAwait(false);
        }

        private readonly IGroupRegistrationT<WritersHub> _events;
    }
}
