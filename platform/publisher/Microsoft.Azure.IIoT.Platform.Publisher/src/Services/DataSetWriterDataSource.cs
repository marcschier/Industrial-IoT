// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Extensions.Options;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Opc.Ua.Client;
    using Opc.Ua;
    using System.Threading;

    /// <summary>
    /// A dataset writer source
    /// </summary>
    public sealed class DataSetWriterDataSource : ISubscriptionListener, IDisposable {

        /// <summary>
        /// Create persistent writer data source
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="state"></param>
        /// <param name="codec"></param>
        /// <param name="sink"></param>
        /// <param name="client"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public DataSetWriterDataSource(ISubscriptionClient client, IDataSetWriterDataSink sink, 
            IDataSetWriterDiagnostics diagnostics, IDataSetWriterStateReporter state, 
            IOptions<DataSetWriterModel> config, IVariantEncoderFactory codec, ILogger logger) {
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Open subscription
        /// </summary>
        /// <returns></returns>
        public async Task OpenAsync() {
            if (_subscription != null) {
                _logger.Warning("Subscription already exists");
                return;
            }

            var model = ToSubscriptionModel(_config.Value);
            var sc = await _client.CreateSubscriptionAsync(
                model, this).ConfigureAwait(false);

            await sc.ApplyAsync(model.MonitoredItems,
                model.Configuration).ConfigureAwait(false);
            _subscription = sc;
        }

        /// <summary>
        /// activate a subscription
        /// </summary>
        /// <returns></returns>
        public async Task ActivateAsync() {
            if (_subscription == null) {
                _logger.Warning("Subscription not registered");
                return;
            }

            // only try to activate if already enabled. Otherwise the activation
            // will be handled by the session's keep alive mechanism
            if (_subscription.Enabled) {
                await _subscription.ActivateAsync(null).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// deactivate a subscription
        /// </summary>
        /// <returns></returns>
        public async Task DeactivateAsync() {
            if (_subscription == null) {
                _logger.Warning("Subscription not registered");
                return;
            }
            await _subscription.CloseAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void OnEndpointConnectivityChange(EndpointConnectivityState previous,
            EndpointConnectivityState newState) {
            var state = new PublishedDataSetSourceStateModel {
                EndpointState = newState,
            };
            _state.OnDataSetWriterStateChange(_config.Value.DataSetWriterId, state);
            if (newState == EndpointConnectivityState.Connecting) {
                _diagnostics.ReportConnectionRetry(_config.Value.DataSetWriterId);
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
                _state.OnDataSetVariableStateChange(_config.Value.DataSetWriterId,
                    monitoredItemId, state);
            }
            else {
                // Report as event state
                _state.OnDataSetEventStateChange(_config.Value.DataSetWriterId, state);
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
            _state.OnDataSetWriterStateChange(_config.Value.DataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, DataChangeNotification notification,
            IList<string> stringTable) {

            _diagnostics.ReportDataSetWriterSubscriptionNotifications(
                _config.Value.DataSetWriterId,
                notification.MonitoredItems.Count);

            _sink.Write(_config.Value, Interlocked.Increment(ref _sequenceNumber), 
                notification, stringTable, subscription);
        }

        /// <inheritdoc/>
        public void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, EventNotificationList notification,
            IList<string> stringTable) {

            _diagnostics.ReportDataSetWriterSubscriptionNotifications(
                _config.Value.DataSetWriterId, notification.Events.Count);

            _sink.Write(_config.Value, Interlocked.Increment(ref _sequenceNumber), 
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
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        private static SubscriptionModel ToSubscriptionModel(DataSetWriterModel dataSetWriter) {
            if (dataSetWriter == null) {
                return null;
            }
            if (dataSetWriter.DataSetWriterId == null) {
                throw new ArgumentException("Missing writer id", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet == null) {
                throw new ArgumentException("Missing dataset", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet.DataSetSource == null) {
                throw new ArgumentException("Missing dataset source", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet.DataSetSource.Connection == null) {
                throw new ArgumentException("Missing dataset connection", nameof(dataSetWriter));
            }
            var monitoredItems = dataSetWriter.DataSet.DataSetSource.ToMonitoredItems();
            return new SubscriptionModel {
                Connection = dataSetWriter.DataSet.DataSetSource.Connection.Clone(),
                Id = dataSetWriter.DataSetWriterId,
                MonitoredItems = monitoredItems,
                ExtensionFields = dataSetWriter.DataSet.ExtensionFields,
                Configuration = dataSetWriter.DataSet.DataSetSource
                    .ToSubscriptionConfigurationModel()
            };
        }

        private ISubscriptionHandle _subscription;
        private volatile uint _sequenceNumber;
        private readonly IVariantEncoderFactory _codec;
        private readonly ISubscriptionClient _client;
        private readonly IDataSetWriterDataSink _sink;
        private readonly IDataSetWriterStateReporter _state;
        private readonly ILogger _logger;
        private readonly IDataSetWriterDiagnostics _diagnostics;
        private readonly IOptions<DataSetWriterModel> _config;
    }
}
