// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Clients {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Messaging;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Send writer group state events
    /// </summary>
    public sealed class WriterGroupStateSender : IWriterGroupStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="events"></param>
        /// <param name="serializer"></param>
        /// <param name="processor"></param>
        /// <param name="logger"></param>
        public WriterGroupStateSender(IEventClient events, IJsonSerializer serializer, 
            ITaskProcessor processor, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void OnWriterGroupStateChange(string writerGroupId, WriterGroupStatus? state) {
            _logger.Information("{writerGroup} changed state to {state}", writerGroupId, state);
            var ev = new WriterGroupStateEventModel {
                WriterGroupId = writerGroupId,
                State = new WriterGroupStateModel {
                    LastState = state ?? WriterGroupStatus.Disabled,
                    LastStateChange = DateTime.UtcNow
                }
            };
            _processor.TrySchedule(() => SendAsync(ev));
        }

        /// <summary>
        /// Send status
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private Task SendAsync(WriterGroupStateEventModel state) {
            return _events.SendEventAsync(null,
                _serializer.SerializeToBytes(state).ToArray(), ContentMimeType.Json,
                MessageSchemaTypes.WriterGroupEvents, "utf-8");
        }

        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly ITaskProcessor _processor;
        private readonly IEventClient _events;
    }
}