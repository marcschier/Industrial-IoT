// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery results are sent through event client
    /// </summary>
    public sealed class DiscoveryResultEventPublisher : IDiscoveryResultHandler {

        /// <summary>
        /// Create services
        /// </summary>
        /// <param name="events"></param>
        /// <param name="serializer"></param>
        public DiscoveryResultEventPublisher(IEventClient events, IJsonSerializer serializer) {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task ReportResultsAsync(
            IEnumerable<DiscoveryResultModel> messages, CancellationToken ct) {
            await Task.Run(() => SendAsync(messages), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Send via events
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private Task SendAsync(IEnumerable<DiscoveryResultModel> messages) {
            var events = messages.Select(message =>
                _serializer.SerializeToBytes(message).ToArray());
            return _events.SendEventAsync(null, events, ContentMimeType.Json,
                MessageSchemaTypes.DiscoveryEvents, "utf-8");
        }

        private readonly IEventClient _events;
        private readonly IJsonSerializer _serializer;
    }
}