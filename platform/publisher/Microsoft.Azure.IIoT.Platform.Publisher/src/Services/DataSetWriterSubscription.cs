// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Threading;
    using Opc.Ua.Client;
    using Opc.Ua;

    /// <summary>
    /// A dataSet writer subsription uses the stack subscription client to 
    /// subscribe to published values and forwards the obtained data to a 
    /// data set writer sink.
    /// </summary>
    public sealed class DataSetWriterSubscription : ISubscriptionListener,
        IDataSetWriterDataSource, IDisposable {

        /// <inheritdoc/>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Create persistent writer data source
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="writerState"></param>
        /// <param name="codec"></param>
        /// <param name="sink"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DataSetWriterSubscription(ISubscriptionClient client, IDataSetWriterDataSink sink,
            IDataSetWriterDiagnostics diagnostics, IDataSetWriterStateReporter writerState,
            IVariantEncoderFactory codec, ILogger logger) {
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _writerState = writerState ?? throw new ArgumentNullException(nameof(writerState));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        /// <inheritdoc/>
        public async Task ConfigureAsync(PublishedDataSetModel dataSet) {
            if (_subscription != null) {
                _logger.Information("Stopping {writer} subscription...", DataSetWriterId);
                await _subscription.CloseAsync().ConfigureAwait(false);
                _subscription = null;
            }

            var config = ToSubscriptionModel(dataSet);
            if (config == null) { // if dataset is null or empty
                _logger.Information("{writer} successfully disabled", DataSetWriterId);
                return;
            }

            _subscription = await _client.CreateSubscriptionAsync(config,
                this).ConfigureAwait(false);
            await _subscription.ApplyAsync(config.MonitoredItems, 
                config.Configuration).ConfigureAwait(false);
            _logger.Information("Started {writer} subscription...", DataSetWriterId);

            //
            // only try to activate if already enabled. Otherwise the activation
            // will be handled by the session's keep alive mechanism.
            //
            if (_subscription.Enabled) {
                await _subscription.ActivateAsync(null).ConfigureAwait(false);
                _logger.Information("Activated {writer} subscription...", DataSetWriterId);
            }

            _logger.Information("{writer} successfully reconfigured.", DataSetWriterId);
        }

        /// <inheritdoc/>
        public void OnConnectivityChange(ConnectionStatus previous, ConnectionStatus newState) {
            var state = new PublishedDataSetSourceStateModel {
                ConnectionState = new ConnectionStateModel {
                    State = newState,
                    LastResultChange = DateTime.UtcNow
                }
            };
            _writerState.OnDataSetWriterStateChange(DataSetWriterId, state);
            if (newState == ConnectionStatus.Connecting) {
                _diagnostics.ReportConnectionRetry(DataSetWriterId);
            }
        }

        /// <inheritdoc/>
        public void OnMonitoredItemStatusChange(string subscriptionId,
            Subscription subscription, string monitoredItemId, bool isEvent,
            uint? clientHandle, uint? serverId, ServiceResult lastResult) {

            var codec = subscription?.Session?.MessageContext == null ? _codec.Default :
                _codec.Create(subscription?.Session.MessageContext);
            var state = new PublishedDataSetItemStateModel {
                LastResultChange = DateTime.UtcNow,
                LastResult = codec.Encode(
                    lastResult?.StatusCode ?? lastResult?.InnerResult?.StatusCode),
                ServerId = serverId,
                ClientId = clientHandle
            };
            if (!isEvent) {
                // Report as variable state change
                _writerState.OnDataSetVariableStateChange(DataSetWriterId,
                    monitoredItemId, state);
            }
            else {
                // Report as event state
                _writerState.OnDataSetEventStateChange(DataSetWriterId, state);
            }
        }

        /// <inheritdoc/>
        public void OnSubscriptionStatusChange(string subscriptionId,
            Subscription subscription, ServiceResult lastResult) {
            var codec = subscription?.Session?.MessageContext == null ? _codec.Default :
                _codec.Create(subscription?.Session.MessageContext);
            var state = new PublishedDataSetSourceStateModel {
                LastResultChange = DateTime.UtcNow,
                LastResult = codec.Encode(
                    lastResult?.StatusCode ?? lastResult?.InnerResult?.StatusCode),
            };
            _writerState.OnDataSetWriterStateChange(DataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, DataChangeNotification notification,
            IList<string> stringTable) {

            _diagnostics.ReportDataSetWriterSubscriptionNotifications(
                DataSetWriterId, notification.MonitoredItems.Count);

            _sink.OnDataSetNotification(DataSetWriterId,
                Interlocked.Increment(ref _sequenceNumber),
                notification, stringTable, subscription);
        }

        /// <inheritdoc/>
        public void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, EventNotificationList notification,
            IList<string> stringTable) {

            _diagnostics.ReportDataSetWriterSubscriptionNotifications(
                DataSetWriterId, notification.Events.Count);

            _sink.OnDataSetNotification(DataSetWriterId,
                Interlocked.Increment(ref _sequenceNumber),
                notification, stringTable, subscription);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _subscription?.Dispose();
            _subscription = null;
        }

        /// <summary>
        /// Create subscription info model from writer model
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        private SubscriptionModel ToSubscriptionModel(PublishedDataSetModel dataSet) {
            if (dataSet == null) {
                return null;
            }
            if (dataSet.DataSetSource == null) {
                throw new ArgumentException("Missing dataSet source", nameof(dataSet));
            }
            if (dataSet.DataSetSource.Connection == null) {
                throw new ArgumentException("Missing dataSet connection", nameof(dataSet));
            }
            var monitoredItems = dataSet.DataSetSource.ToMonitoredItems();
            return new SubscriptionModel {
                Connection = dataSet.DataSetSource.Connection.Clone(),
                Id = DataSetWriterId,
                MonitoredItems = monitoredItems,
                ExtensionFields = dataSet.ExtensionFields,
                Configuration = dataSet.DataSetSource.ToSubscriptionConfigurationModel()
            };
        }

        private ISubscriptionHandle _subscription;
        private volatile uint _sequenceNumber;
        private readonly IVariantEncoderFactory _codec;
        private readonly ISubscriptionClient _client;
        private readonly IDataSetWriterDataSink _sink;
        private readonly IDataSetWriterStateReporter _writerState;
        private readonly ILogger _logger;
        private readonly IDataSetWriterDiagnostics _diagnostics;
    }
}
