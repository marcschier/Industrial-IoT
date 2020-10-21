// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using Opc.Ua.Client;
    using Opc.Ua;

    /// <summary>
    /// Simple in memory writer group data source.  Manages serveral data
    /// source subscriptions in a dictionary and adds or removes based on 
    /// the configuration provided by control client.
    /// </summary>
    public sealed class SimpleWriterGroupDataSource : IWriterGroupDataSource, 
        IDataSetWriterDataSink, IDisposable {

        /// <inheritdoc/>
        public string WriterGroupId { get; set; }

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
        /// <param name="writerState"></param>
        /// <param name="groupState"></param>
        /// <param name="codec"></param>
        /// <param name="sink"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public SimpleWriterGroupDataSource(ISubscriptionClient client, IWriterGroupDataSink sink, 
            IDataSetWriterDiagnostics diagnostics, IDataSetWriterStateReporter writerState, 
            IWriterGroupStateReporter groupState, IVariantEncoderFactory codec, ILogger logger) {

            _writerState = writerState ?? throw new ArgumentNullException(nameof(writerState));
            _groupState = groupState ?? throw new ArgumentNullException(nameof(groupState));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _sources = new ConcurrentDictionary<string, Task<DataSetWriterSubscription>>();
            _writers = new ConcurrentDictionary<string, DataSetWriterModel>();
        }

        /// <inheritdoc/>
        public void AddWriters(IEnumerable<DataSetWriterModel> dataSetWriters) {
            if (dataSetWriters is null) {
                throw new ArgumentNullException(nameof(dataSetWriters));
            }

            foreach (var writer in dataSetWriters) {
                _sources.AddOrUpdate(writer.DataSetWriterId, async writerId => {
                    var subscription = new DataSetWriterSubscription(
                        _client, this, _diagnostics, _writerState, _codec, _logger) {
                        DataSetWriterId = writer.DataSetWriterId
                    };
                    await subscription.ConfigureAsync(writer.DataSet).ConfigureAwait(false);
                    return subscription;
                }, async (writerId, task) => {
                    DataSetWriterSubscription subscription = null;
                    try {
                        // First get subscription
                        subscription = await task.ConfigureAwait(false);
                    }
                    catch {
                        // Failed, create new
                        subscription = new DataSetWriterSubscription(
                            _client, this, _diagnostics, _writerState, _codec, _logger) {
                            DataSetWriterId = writer.DataSetWriterId
                        };
                    }
                    finally {
                        subscription?.Dispose();
                    }
                    await subscription.ConfigureAsync(writer.DataSet).ConfigureAwait(false);
                    return subscription;
                });
                _writers.AddOrUpdate(writer.DataSetWriterId, writer, (k,v) => writer);
            }

            if (!_writers.IsEmpty) {
                _groupState.OnWriterGroupStateChange(WriterGroupId, WriterGroupStatus.Publishing);
            }
        }

        /// <inheritdoc/>
        public void RemoveWriters(IEnumerable<string> dataSetWriters) {
            if (dataSetWriters is null) {
                throw new ArgumentNullException(nameof(dataSetWriters));
            }

            foreach (var writer in dataSetWriters) {
                if (_sources.TryRemove(writer, out var subscription)) {
                    DisposeAsync(subscription).Wait();
                }
                _writers.TryRemove(writer, out _);
            }

            if (_writers.IsEmpty) {
                _groupState.OnWriterGroupStateChange(WriterGroupId, WriterGroupStatus.Pending);
            }
        }

        /// <inheritdoc/>
        public void RemoveAllWriters() {
            var writers = _sources.Values.ToList();
            _sources.Clear();
            _writers.Clear();

            Task.WhenAll(writers.Select(sc => DisposeAsync(sc))).Wait();
            _groupState.OnWriterGroupStateChange(WriterGroupId, WriterGroupStatus.Pending);
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
                _sources.Clear();
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
                await subscription.ConfigureAsync(null).ConfigureAwait(false);
            }
            catch { }
            finally {
                subscription?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void OnDataSetNotification(string dataSetWriterId, PublishedDataSetModel dataSet, 
            uint sequenceNumber, NotificationData notification, IList<string> stringTable,
            Subscription subscription) {
            if (notification is not DataChangeNotification values) {
                throw new NotSupportedException(); // TODO
            }

            if (!_writers.TryGetValue(dataSetWriterId, out var writer)) {
                return; // No settings
            }

            _sink.Enqueue(new DataSetWriterMessageModel {
                Notifications = values.ToMonitoredItemNotifications(
                    subscription.MonitoredItems).ToList(),
                ServiceMessageContext = subscription.Session.MessageContext,
                SequenceNumber = sequenceNumber,
                ApplicationUri = subscription.Session.Endpoint.Server.ApplicationUri,
                EndpointUrl = subscription.Session.Endpoint.EndpointUrl,
                TimeStamp = DateTime.UtcNow,
                Writer = writer
            });
        }

        // Services
        private readonly ILogger _logger;
        private readonly IVariantEncoderFactory _codec;
        private readonly ISubscriptionClient _client;
        private readonly IWriterGroupDataSink _sink;
        private readonly IDataSetWriterStateReporter _writerState;
        private readonly IWriterGroupStateReporter _groupState;
        private readonly ConcurrentDictionary<string, Task<DataSetWriterSubscription>> _sources;
        private readonly ConcurrentDictionary<string, DataSetWriterModel> _writers;
        private readonly IDataSetWriterDiagnostics _diagnostics;
    }
}
