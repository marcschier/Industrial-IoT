﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Subscriber.Handlers {
    using Microsoft.Azure.IIoT.Platform.Subscriber;
    using Microsoft.Azure.IIoT.Platform.Subscriber.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Publisher message handling
    /// </summary>
    public sealed class MonitoredItemSampleModelHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => Core.MessageSchemaTypes.MonitoredItemMessageModelJson;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public MonitoredItemSampleModelHandler(IEnumerable<ISubscriberMessageProcessor> handlers,
            IJsonSerializer serializer, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _handlers = handlers?.ToList() ?? throw new ArgumentNullException(nameof(handlers));
        }

        /// <inheritdoc/>
        public async Task HandleAsync(string source, byte[] payload,
            IDictionary<string, string> properties, Func<Task> checkpoint) {
            try {
                var sample = _serializer.Deserialize<MonitoredItemMessageModel>(payload);
                await Task.WhenAll(_handlers.Select(h => h.HandleSampleAsync(sample))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Exception handling message from {source}", source);
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly List<ISubscriberMessageProcessor> _handlers;
    }
}
