// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Events.v2 {
    using Microsoft.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher registry event publisher
    /// </summary>
    public class PublisherEventBusPublisher : IPublisherRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public PublisherEventBusPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnPublisherDeletedAsync(OperationContextModel context,
            string publisherId) {
            return _bus.PublishAsync(Wrap(PublisherEventType.Deleted, context,
                publisherId, null));
        }

        /// <inheritdoc/>
        public Task OnPublisherNewAsync(OperationContextModel context,
            PublisherModel publisher) {
            return _bus.PublishAsync(Wrap(PublisherEventType.New, context,
                publisher.Id, publisher));
        }

        /// <inheritdoc/>
        public Task OnPublisherUpdatedAsync(OperationContextModel context,
            PublisherModel publisher) {
            return _bus.PublishAsync(Wrap(PublisherEventType.Updated, context,
                publisher.Id, publisher));
        }

        /// <summary>
        /// Create publisher event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="publisherId"></param>
        /// <param name="publisher"></param>
        /// <returns></returns>
        private static PublisherEventModel Wrap(PublisherEventType type,
            OperationContextModel context, string publisherId,
            PublisherModel publisher) {
            return new PublisherEventModel {
                EventType = type,
                Context = context,
                Id = publisherId,
                Publisher = publisher
            };
        }

        private readonly IEventBusPublisher _bus;
    }
}
