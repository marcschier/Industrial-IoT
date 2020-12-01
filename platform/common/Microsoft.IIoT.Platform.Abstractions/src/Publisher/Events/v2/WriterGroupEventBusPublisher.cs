// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Events.v2 {
    using Microsoft.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer group registry event publisher
    /// </summary>
    public class WriterGroupEventBusPublisher : IWriterGroupRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public WriterGroupEventBusPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }


        /// <inheritdoc/>
        public Task OnWriterGroupAddedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.Added, context,
                writerGroup.WriterGroupId, writerGroup));
        }

        /// <inheritdoc/>
        public Task OnWriterGroupUpdatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.Updated, context,
                writerGroup.WriterGroupId, writerGroup));
        }

        /// <inheritdoc/>
        public Task OnWriterGroupStateChangeAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.StateChange, context,
                writerGroup.WriterGroupId, writerGroup));
        }

        /// <inheritdoc/>
        public Task OnWriterGroupActivatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.Activated, context,
                writerGroup.WriterGroupId, writerGroup));
        }

        /// <inheritdoc/>
        public Task OnWriterGroupDeactivatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.Deactivated, context,
                writerGroup.WriterGroupId, writerGroup));
        }

        /// <inheritdoc/>
        public Task OnWriterGroupRemovedAsync(OperationContextModel context,
            string writerGroupId) {
            return _bus.PublishAsync(Wrap(WriterGroupEventType.Removed, context,
                writerGroupId, null));
        }

        /// <summary>
        /// Create writer group event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="writerGroupId"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        private static WriterGroupEventModel Wrap(WriterGroupEventType type,
            OperationContextModel context, string writerGroupId,
            WriterGroupInfoModel writerGroup) {
            return new WriterGroupEventModel {
                EventType = type,
                Context = context,
                Id = writerGroupId,
                WriterGroup = writerGroup
            };
        }

        private readonly IEventBusPublisher _bus;
    }
}
