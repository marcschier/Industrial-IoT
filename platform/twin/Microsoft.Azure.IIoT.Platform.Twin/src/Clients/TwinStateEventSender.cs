// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Send twin state events
    /// </summary>
    public sealed class TwinStateEventSender : ITwinStateReporter {

        /// <summary>
        /// Create sender
        /// </summary>
        /// <param name="events"></param>
        /// <param name="serializer"></param>
        /// <param name="processor"></param>
        /// <param name="logger"></param>
        public TwinStateEventSender(IEventClient events, IJsonSerializer serializer, 
            ITaskProcessor processor, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _logger = new TwinStateEventLogger(logger);
        }

        /// <inheritdoc/>
        public void OnConnectionStateChange(string twinId, ConnectionStateModel state) {
            _logger.OnConnectionStateChange(twinId, state);
            var ev = new TwinStateEventModel {
                TwinId = twinId,
                ConnectionState = state
            };
            _processor.TrySchedule(() => SendAsync(ev));
        }

        /// <summary>
        /// Send status
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private Task SendAsync(TwinStateEventModel state) {
            return _events.SendEventAsync(null,
                _serializer.SerializeToBytes(state).ToArray(), ContentMimeType.Json,
                MessageSchemaTypes.TwinEvents, "utf-8");
        }

        private readonly TwinStateEventLogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly ITaskProcessor _processor;
        private readonly IEventClient _events;
    }
}