// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Edge.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Messaging;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery progress message sender
    /// </summary>
    public class ProgressPublisher : ProgressLogger, IDiscoveryProgress {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="events"></param>
        /// <param name="identity"></param>
        /// <param name="processor"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public ProgressPublisher(IEventClient events, IIdentity identity,
            ITaskProcessor processor, IJsonSerializer serializer, ILogger logger) :
            base (logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        /// <summary>
        /// Send progress
        /// </summary>
        /// <param name="progress"></param>
        protected override void Send(DiscoveryProgressModel progress) {
            progress.DiscovererId = DiscovererModelEx.CreateDiscovererId(_identity.DeviceId,
                _identity.ModuleId);
            base.Send(progress);
            _processor.TrySchedule(() => SendAsync(progress));
        }

        /// <summary>
        /// Send progress
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private Task SendAsync(DiscoveryProgressModel progress) {
            return Try.Async(() => _events.SendEventAsync(null, // TODO: Target
                _serializer.SerializeToBytes(progress).ToArray(), ContentMimeType.Json,
                MessageSchemaTypes.DiscoveryMessage, "utf-8"));
        }

        private readonly IJsonSerializer _serializer;
        private readonly IEventClient _events;
        private readonly IIdentity _identity;
        private readonly ITaskProcessor _processor;
    }
}
