// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Events.v2 {
    using Microsoft.IIoT.Platform.Twin.Events.v2.Models;
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin registry event publisher
    /// </summary>
    public class TwinEventBusPublisher : ITwinRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public TwinEventBusPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnTwinActivatedAsync(
            OperationContextModel context, TwinInfoModel twin) {
            return _bus.PublishAsync(Wrap(TwinEventType.Activated, context,
                twin));
        }

        /// <inheritdoc/>
        public Task OnTwinUpdatedAsync(
            OperationContextModel context, TwinInfoModel twin) {
            return _bus.PublishAsync(Wrap(TwinEventType.Updated, context,
                twin));
        }

        /// <inheritdoc/>
        public Task OnTwinDeactivatedAsync(
            OperationContextModel context, TwinInfoModel twin) {
            return _bus.PublishAsync(Wrap(TwinEventType.Deactivated, context,
                twin));
        }

        /// <summary>
        /// Create twin event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        private static TwinEventModel Wrap(TwinEventType type,
            OperationContextModel context, TwinInfoModel twin) {
            return new TwinEventModel {
                EventType = type,
                Context = context,
                Twin = twin
            };
        }

        private readonly IEventBusPublisher _bus;
    }
}
