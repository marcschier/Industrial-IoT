// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Diagnostics;

    /// <summary>
    /// Simple event processor host
    /// </summary>
    public sealed class SimpleEventProcessor : HostProcess, IEventProcessingHost {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="handler"></param>
        /// <param name="logger"></param>
        public SimpleEventProcessor(IEventConsumer consumer,
            IEventProcessingHandler handler, ILogger logger) : base (logger, "Consumer") {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _sw = Stopwatch.StartNew();
        }

        /// <summary>
        /// Consumer loop
        /// </summary>
        /// <param name="ct"></param>
        protected override async Task RunAsync(CancellationToken ct) {
            _sw.Restart();
            while (!ct.IsCancellationRequested) {
                try {
                    while (!ct.IsCancellationRequested) {
                        var messages = await _consumer.ConsumeAsync(ct).ConfigureAwait(false);
                        foreach (var message in messages) {
                            await _handler.HandleAsync(message.Item1, message.Item2,
                                () => Task.CompletedTask).ConfigureAwait(false);
                        }
                        await Try.Async(_handler.OnBatchCompleteAsync).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception error) {
                    // Exception - report and continue
                    _logger.LogWarning(error, "Consumer encountered error...");
                    continue;
                }
            }
            _logger.LogInformation("Exiting consumer...");
        }

        private readonly ILogger _logger;
        private readonly IEventProcessingHandler _handler;
        private readonly IEventConsumer _consumer;
        private readonly Stopwatch _sw;
    }
}
