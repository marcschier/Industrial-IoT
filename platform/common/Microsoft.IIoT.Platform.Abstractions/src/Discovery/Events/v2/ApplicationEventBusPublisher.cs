// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Events.v2 {
    using Microsoft.IIoT.Platform.Discovery.Events.v2.Models;
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Application registry event publisher
    /// </summary>
    public class ApplicationEventBusPublisher : IApplicationRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public ApplicationEventBusPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnApplicationDeletedAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return _bus.PublishAsync(Wrap(ApplicationEventType.Deleted, context,
                application));
        }

        /// <inheritdoc/>
        public Task OnApplicationNewAsync(
            OperationContextModel context, ApplicationInfoModel application) {
            return _bus.PublishAsync(Wrap(ApplicationEventType.New, context,
                application));
        }

        /// <inheritdoc/>
        public Task OnApplicationFoundAsync(
            OperationContextModel context, ApplicationInfoModel application) {
            return _bus.PublishAsync(Wrap(ApplicationEventType.Found, context,
                application));
        }

        /// <inheritdoc/>
        public Task OnApplicationLostAsync(
            OperationContextModel context, ApplicationInfoModel application) {
            return _bus.PublishAsync(Wrap(ApplicationEventType.Lost, context,
                application));
        }

        /// <inheritdoc/>
        public Task OnApplicationUpdatedAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return _bus.PublishAsync(Wrap(ApplicationEventType.Updated, context,
                application));
        }

        /// <summary>
        /// Create application event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="application"></param>
        ///
        /// <returns></returns>
        private static ApplicationEventModel Wrap(ApplicationEventType type,
            OperationContextModel context, ApplicationInfoModel application) {
            return new ApplicationEventModel {
                EventType = type,
                Context = context,
                Application = application
            };
        }

        private readonly IEventBusPublisher _bus;
    }
}
