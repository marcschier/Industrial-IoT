// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Discovery.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint registry event publisher
    /// </summary>
    public class EndpointEventBusPublisher : IEndpointRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public EndpointEventBusPublisher(IEventBus bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnEndpointDeletedAsync(
            OperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Deleted, context,
                endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointNewAsync(
            OperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.New, context,
                endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointFoundAsync(
            OperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Found, context,
                endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointLostAsync(
            OperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Lost, context,
                endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointUpdatedAsync(
            OperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Updated, context,
                endpoint));
        }

        /// <summary>
        /// Create endpoint event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private static EndpointEventModel Wrap(EndpointEventType type,
            OperationContextModel context, EndpointInfoModel endpoint) {
            return new EndpointEventModel {
                EventType = type,
                Context = context,
                Endpoint = endpoint
            };
        }

        private readonly IEventBus _bus;
    }
}
