﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Gateway registry event gateway
    /// </summary>
    public class GatewayEventBusPublisher : IGatewayRegistryListener {

        /// <summary>
        /// Create event gateway
        /// </summary>
        /// <param name="bus"></param>
        public GatewayEventBusPublisher(IEventBus bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnGatewayDeletedAsync(OperationContextModel context,
            string gatewayId) {
            return _bus.PublishAsync(Wrap(GatewayEventType.Deleted, context,
                gatewayId, null));
        }

        /// <inheritdoc/>
        public Task OnGatewayNewAsync(OperationContextModel context,
            GatewayModel gateway) {
            return _bus.PublishAsync(Wrap(GatewayEventType.New, context,
                gateway.Id, gateway));
        }

        /// <inheritdoc/>
        public Task OnGatewayUpdatedAsync(OperationContextModel context,
            GatewayModel gateway) {
            return _bus.PublishAsync(Wrap(GatewayEventType.Updated, context,
                gateway.Id, gateway));
        }

        /// <summary>
        /// Create gateway event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="gatewayId"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        private static GatewayEventModel Wrap(GatewayEventType type,
            OperationContextModel context, string gatewayId,
            GatewayModel gateway) {
            return new GatewayEventModel {
                EventType = type,
                Context = context,
                Id = gatewayId,
                Gateway = gateway
            };
        }

        private readonly IEventBus _bus;
    }
}
