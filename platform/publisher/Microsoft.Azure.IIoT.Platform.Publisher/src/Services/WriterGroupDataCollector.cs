// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Collects / receives data for writers in the writer group, contextualizes
    /// them and enqueues them to the writer group message emitter.
    /// </summary>
    public sealed class WriterGroupDataCollector : IWriterGroupMessageCollector,
        IDisposable {

        /// <inheritdoc/>
        public uint? GroupVersion { get; set; }

        /// <inheritdoc/>
        public double? SamplingOffset { get; set; }

        /// <inheritdoc/>
        public TimeSpan? KeepAliveTime { get; set; }

        /// <inheritdoc/>
        public byte? Priority { get; set; }

        /// <summary>
        /// Create writer group processor
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="state"></param>
        /// <param name="codec"></param>
        /// <param name="sender"></param>
        /// <param name="subscriptions"></param>
        /// <param name="logger"></param>
        public WriterGroupDataCollector(ISubscriptionClient subscriptions,
            IDataSetMessageSender sender, IDataSetWriterDiagnostics diagnostics,
            IDataSetWriterStateReporter state, IVariantEncoderFactory codec, 
            ILogger logger) {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _writers = new ConcurrentDictionary<string, Task<DataSetWriterSubscription>>();
        }

        /// <inheritdoc/>
        public void AddWriters(IEnumerable<DataSetWriterModel> dataSetWriters) {
            if (dataSetWriters is null) {
                throw new ArgumentNullException(nameof(dataSetWriters));
            }

            // TODO capture tasks

            foreach (var writer in dataSetWriters) {
                _writers.AddOrUpdate(writer.DataSetWriterId, async writerId => {
                    var subscription = new DataSetWriterSubscription(
                        _subscriptions, null, _diagnostics, _state, Options.Create(writer), _codec, _logger);
                    await subscription.OpenAsync().ConfigureAwait(false);
                    await subscription.ActivateAsync().ConfigureAwait(false);
                    return subscription;
                }, async (writerId, old) => {
                    DataSetWriterSubscription subscription = null;
                    try {
                        subscription = await old.ConfigureAwait(false);
                        await subscription.DeactivateAsync().ConfigureAwait(false);
                    }
                    catch { }
                    finally {
                        subscription?.Dispose();
                    }
                    subscription = new DataSetWriterSubscription(
                        _subscriptions, null, _diagnostics, _state, Options.Create(writer), _codec, _logger);
                    await subscription.OpenAsync().ConfigureAwait(false);
                    await subscription.ActivateAsync().ConfigureAwait(false);
                    return subscription;
                });
            }
        }

        /// <inheritdoc/>
        public void RemoveWriters(IEnumerable<string> dataSetWriters) {
            if (dataSetWriters is null) {
                throw new ArgumentNullException(nameof(dataSetWriters));
            }

            // TODO capture tasks

            foreach (var writer in dataSetWriters) {
                if (_writers.TryRemove(writer, out var subscription)) {
                    DisposeAsync(subscription).Wait();
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveAllWriters() {

            // TODO capture tasks

            var writers = _writers.Values.ToList();
            _writers.Clear();
            Task.WhenAll(writers.Select(sc => DisposeAsync(sc))).Wait();
        }

        /// <inheritdoc/>
        public void Dispose() {
            try {
                // Stop
                RemoveAllWriters();
            }
            catch {
                // Nothing...
            }
            finally {
                _writers.Clear();
            }
        }

        /// <summary>
        /// Dispose writer
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        private static async Task DisposeAsync(Task<DataSetWriterSubscription> writer) {
            DataSetWriterSubscription subscription = null;
            try {
                // TODO: Add cleanup
                subscription = await writer.ConfigureAwait(false);
                await subscription.DeactivateAsync().ConfigureAwait(false);
            }
            catch { }
            finally {
                subscription?.Dispose();
            }
        }

        // Services
        private readonly IVariantEncoderFactory _codec;
        private readonly ISubscriptionClient _subscriptions;
        private readonly IDataSetMessageSender _sender;
        private readonly IDataSetWriterStateReporter _state;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Task<DataSetWriterSubscription>> _writers;
        private readonly IDataSetWriterDiagnostics _diagnostics;
    }
}
