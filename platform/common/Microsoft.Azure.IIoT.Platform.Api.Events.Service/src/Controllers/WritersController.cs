// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Api.Events.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Api.Events.Service.Filters;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset writer monitoring services
    /// </summary>
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/writers")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class WritersController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="events"></param>
        public WritersController(IGroupRegistrationT<DataSetWritersHub> events) {
            _events = events;
        }

        /// <summary>
        /// Subscribe to receive dataset writer item status updates
        /// </summary>
        /// <remarks>
        /// Register a client to receive status updates for variables and events
        /// in the dataset through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to subscribe to
        /// </param>
        /// <param name="connectionId">The connection that will receive status
        /// updates.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/status")]
        public async Task SubscribeAsync(string dataSetWriterId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(dataSetWriterId, connectionId);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset writer item status updates.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving status updates
        /// for variables and events in the dataset.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to unsubscribe
        /// from </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more status updates.</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/status/{connectionId}")]
        public async Task UnsubscribeAsync(string dataSetWriterId, string connectionId) {
            await _events.UnsubscribeAsync(dataSetWriterId, connectionId);
        }

        private readonly IGroupRegistrationT<DataSetWritersHub> _events;
    }
}
