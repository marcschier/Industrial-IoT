// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.IIoT.Azure.IoTEdge;
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Utils;
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.IIoT.Hosting;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Prometheus;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Diagnostics.Tracing;
    using System.Globalization;

    /// <summary>
    /// Injectable IoT Sdk client
    /// </summary>
    public sealed class IoTEdgeHubClient : IIoTEdgeClient, IDisposable {

        /// <summary>
        /// Create sdk factory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="identity"></param>
        /// <param name="broker"></param>
        /// <param name="ctrl"></param>
        /// <param name="logger"></param>
        public IoTEdgeHubClient(IOptions<IoTEdgeClientOptions> options, IIdentity identity,
            IEventSourceBroker broker, IProcessControl ctrl, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ctrl = ctrl ?? throw new ArgumentNullException(nameof(ctrl));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));

            if (broker != null) {
                _logHook = broker.Subscribe(IoTSdkLogger.EventSource, new IoTSdkLogger(logger));
            }

            var bypassCertValidation = _options.Value.BypassCertVerification;
            if (!bypassCertValidation) {
                if (!string.IsNullOrEmpty(identity.Gateway)) {
                    bypassCertValidation = true;
                }
            }
            TransportOption transportToUse;
            if (!string.IsNullOrEmpty(identity.Gateway)) {
                //
                // Running in edge mode
                // We force the configured transport (if provided) to it's OverTcp
                // variant as follows: AmqpOverTcp when Amqp, AmqpOverWebsocket or
                // AmqpOverTcp specified and MqttOverTcp otherwise.
                // Default is MqttOverTcp
                //
                if ((_options.Value.Transport & TransportOption.Mqtt) != 0) {
                    // prefer Mqtt over Amqp due to performance reasons
                    transportToUse = TransportOption.MqttOverTcp;
                }
                else {
                    transportToUse = TransportOption.AmqpOverTcp;
                }
                _logger.LogInformation("Connecting all clients to {edgeHub} using {transport}.",
                    identity.Gateway, transportToUse);
            }
            else {
                transportToUse = _options.Value.Transport == 0 ?
                    TransportOption.Any : _options.Value.Transport;
            }

            if (bypassCertValidation) {
                _logger.LogWarning("Bypassing certificate validation for client.");
            }
            var transportSettings = GetTransportSettings(bypassCertValidation,
                transportToUse);
            var cs = string.IsNullOrEmpty(options.Value.EdgeHubConnectionString) ? null :
                IotHubConnectionStringBuilder.Create(options.Value.EdgeHubConnectionString);
            _client = CreateAdapterAsync(cs, transportSettings);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string route, Message message,
            CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            await client.SendEventAsync(route, message, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SendEventBatchAsync(string route,
            IEnumerable<Message> messages, CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            await client.SendEventBatchAsync(route, messages, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetMethodDefaultHandlerAsync(
            MethodCallback methodHandler, object userContext) {
            var client = await _client.ConfigureAwait(false);
            await client.SetMethodDefaultHandlerAsync(methodHandler, userContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetMethodHandlerAsync(string methodName,
            MethodCallback methodHandler, object userContext) {
            var client = await _client.ConfigureAwait(false);
            await client.SetMethodHandlerAsync(methodName, methodHandler, userContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Twin> GetTwinAsync(CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            return await client.GetTwinAsync(ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetDesiredPropertyUpdateCallbackAsync(
            DesiredPropertyUpdateCallback callback, object userContext) {
            var client = await _client.ConfigureAwait(false);
            await client.SetDesiredPropertyUpdateCallbackAsync(callback, userContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateReportedPropertiesAsync(
            TwinCollection reportedProperties, CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            await client.UpdateReportedPropertiesAsync(reportedProperties, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodResponse> InvokeMethodAsync(
            string deviceId, string moduleId,
            MethodRequest methodRequest, CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            return await client.InvokeMethodAsync(deviceId, moduleId, methodRequest, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodResponse> InvokeMethodAsync(
            string deviceId, MethodRequest methodRequest, CancellationToken ct) {
            var client = await _client.ConfigureAwait(false);
            return await client.InvokeMethodAsync(deviceId, methodRequest, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task CloseAsync() {
            var client = await _client.ConfigureAwait(false);
            if (client != null) {
                await client.CloseAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            var client = Try.Op(() => _client?.Result);
            if (client != null) {
                try {
                    client?.CloseAsync().Wait();
                }
                catch { }
                finally {
                    (client as IDisposable)?.Dispose();
                }
            }
            _logHook?.Dispose();
        }

        /// <summary>
        /// Get transport settings list for transport
        /// </summary>
        /// <param name="bypassCertValidation"></param>
        /// <param name="transport"></param>
        /// <returns></returns>
        private static List<ITransportSettings> GetTransportSettings(
            bool bypassCertValidation, TransportOption transport) {
            // Configure transport settings
            var transportSettings = new List<ITransportSettings>();
            if ((transport & TransportOption.MqttOverTcp) != 0) {
                var setting = new MqttTransportSettings(
                    TransportType.Mqtt_Tcp_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
                        (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.MqttOverWebsocket) != 0) {
                var setting = new MqttTransportSettings(
                    TransportType.Mqtt_WebSocket_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
                        (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.AmqpOverTcp) != 0) {
                var setting = new AmqpTransportSettings(
                    TransportType.Amqp_Tcp_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
                        (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.AmqpOverWebsocket) != 0) {
                var setting = new AmqpTransportSettings(
                    TransportType.Amqp_WebSocket_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
                        (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                }
                transportSettings.Add(setting);
            }
            return transportSettings;
        }

        /// <summary>
        /// Create client adapter
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private async Task<IIoTEdgeClient> CreateAdapterAsync(
            IotHubConnectionStringBuilder cs, List<ITransportSettings> settings) {
            if (settings.Count != 0) {
                return await Try.Options(settings
                    .Select<ITransportSettings, Func<Task<IIoTEdgeClient>>>(t =>
                         () => CreateAdapterAsync(cs, t))
                    .ToArray()).ConfigureAwait(false);
            }
            return await CreateAdapterAsync(cs, (ITransportSettings)null).ConfigureAwait(false);
        }

        /// <summary>
        /// Create client adapter
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        private async Task<IIoTEdgeClient> CreateAdapterAsync(
            IotHubConnectionStringBuilder cs, ITransportSettings setting) {
            var timeout = TimeSpan.FromMinutes(5);
            if (string.IsNullOrEmpty(_identity.ModuleId)) {
                if (cs == null) {
                    throw new InvalidConfigurationException(
                        "No connection string for device client specified.");
                }
                return await DeviceClientAdapter.CreateAsync(_options.Value.Product, cs,
                    _identity.DeviceId, setting, timeout,
                        () => _ctrl?.Reset(), _logger).ConfigureAwait(false);
            }
            return await ModuleClientAdapter.CreateAsync(_options.Value.Product, cs,
                _identity.DeviceId, _identity.ModuleId, setting, timeout,
                    () => _ctrl?.Reset(), _logger).ConfigureAwait(false);
        }

        /// <summary>
        /// Adapts module client to interface
        /// </summary>
        internal sealed class ModuleClientAdapter : IIoTEdgeClient {

            /// <summary>
            /// Whether the client is closed
            /// </summary>
            public bool IsClosed { get; internal set; }

            /// <summary>
            /// Create client
            /// </summary>
            /// <param name="client"></param>
            private ModuleClientAdapter(ModuleClient client) {
                _client = client ?? throw new ArgumentNullException(nameof(client));
            }

            /// <summary>
            /// Factory
            /// </summary>
            /// <param name="product"></param>
            /// <param name="cs"></param>
            /// <param name="deviceId"></param>
            /// <param name="moduleId"></param>
            /// <param name="transportSetting"></param>
            /// <param name="timeout"></param>
            /// <param name="onConnectionLost"></param>
            /// <param name="logger"></param>
            /// <returns></returns>
            public static async Task<IIoTEdgeClient> CreateAsync(string product,
                IotHubConnectionStringBuilder cs, string deviceId, string moduleId,
                ITransportSettings transportSetting,
                TimeSpan timeout, Action onConnectionLost, ILogger logger) {

                if (cs == null) {
                    logger.LogInformation("Running in iotedge context.");
                }
                else {
                    logger.LogInformation("Running outside iotedge context.");
                }

                var client = await CreateAsync(cs, transportSetting).ConfigureAwait(false);
                var adapter = new ModuleClientAdapter(client);
                try {
                    // Configure
                    client.OperationTimeoutInMilliseconds = (uint)timeout.TotalMilliseconds;
                    client.SetConnectionStatusChangesHandler((s, r) =>
                        adapter.OnConnectionStatusChange(deviceId, moduleId, onConnectionLost,
                            logger, s, r));
                    client.ProductInfo = product;
                    await client.OpenAsync().ConfigureAwait(false);
                    return adapter;
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task CloseAsync() {
                if (IsClosed) {
                    return;
                }
                try {
                    _client.OperationTimeoutInMilliseconds = 3000;
                    _client.SetRetryPolicy(new NoRetry());
                    IsClosed = true;
                    await _client.CloseAsync().ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SendEventAsync(string route, Message message,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                try {
                    if (route != null) {
                        await _client.SendEventAsync(route, message,
                            ct).ConfigureAwait(false);
                    }
                    else {
                        await _client.SendEventAsync(message, ct).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SendEventBatchAsync(string route, IEnumerable<Message> messages,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                try {
                    if (route != null) {
                        await _client.SendEventBatchAsync(route,
                            messages, ct).ConfigureAwait(false);
                    }
                    else {
                        await _client.SendEventBatchAsync(messages, ct).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodHandlerAsync(string methodName,
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodHandlerAsync(methodName,
                        methodHandler, userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodDefaultHandlerAsync(
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodDefaultHandlerAsync(methodHandler,
                        userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetDesiredPropertyUpdateCallbackAsync(
                DesiredPropertyUpdateCallback callback, object userContext) {
                try {
                    await _client.SetDesiredPropertyUpdateCallbackAsync(
                        callback, userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<Twin> GetTwinAsync(CancellationToken ct) {
                try {
                    return await _client.GetTwinAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                try {
                    await _client.UpdateReportedPropertiesAsync(
                        reportedProperties, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId,
                MethodRequest methodRequest, CancellationToken ct) {
                try {
                    return await _client.InvokeMethodAsync(deviceId,
                        moduleId, methodRequest, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<MethodResponse> InvokeMethodAsync(string deviceId,
                MethodRequest methodRequest, CancellationToken ct) {
                try {
                    return await _client.InvokeMethodAsync(deviceId,
                        methodRequest, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public void Dispose() {
                IsClosed = true;
                _client?.Dispose();
            }

            /// <summary>
            /// Handle status change event
            /// </summary>
            /// <param name="deviceId"></param>
            /// <param name="moduleId"></param>
            /// <param name="onConnectionLost"></param>
            /// <param name="logger"></param>
            /// <param name="status"></param>
            /// <param name="reason"></param>
            private void OnConnectionStatusChange(string deviceId, string moduleId,
                Action onConnectionLost, ILogger logger, ConnectionStatus status,
                ConnectionStatusChangeReason reason) {

                if (status == ConnectionStatus.Connected) {
                    logger.LogInformation("{counter}: Module {deviceId}_{moduleId} reconnected " +
                        "due to {reason}.", _reconnectCounter, deviceId, moduleId, reason);
                    kReconnectionStatus.WithLabels(moduleId, deviceId,
                        DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
                        .Set(_reconnectCounter);
                    _reconnectCounter++;
                    return;
                }
                kDisconnectionStatus.WithLabels(moduleId, deviceId,
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
                    .Set(_reconnectCounter);
                logger.LogInformation("{counter}: Module {deviceId}_{moduleId} disconnected " +
                    "due to {reason} - now {status}...", _reconnectCounter, deviceId, moduleId,
                        reason, status);
                if (IsClosed) {
                    // Already closed - nothing to do
                    return;
                }
                if (status == ConnectionStatus.Disconnected ||
                    status == ConnectionStatus.Disabled) {
                    // Force
                    IsClosed = true;
                    onConnectionLost?.Invoke();
                }
            }

            /// <summary>
            /// Helper to create module client
            /// </summary>
            /// <param name="cs"></param>
            /// <param name="transportSetting"></param>
            /// <returns></returns>
            private static async Task<ModuleClient> CreateAsync(IotHubConnectionStringBuilder cs,
                ITransportSettings transportSetting) {
                try {
                    if (transportSetting == null) {
                        if (cs == null) {
                            return await ModuleClient.CreateFromEnvironmentAsync().ConfigureAwait(false);
                        }
                        return ModuleClient.CreateFromConnectionString(cs.ToString());
                    }
                    var ts = new ITransportSettings[] { transportSetting };
                    if (cs == null) {
                        return await ModuleClient.CreateFromEnvironmentAsync(ts).ConfigureAwait(false);
                    }
                    return ModuleClient.CreateFromConnectionString(cs.ToString(), ts);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            private readonly ModuleClient _client;
            private int _reconnectCounter;
            private static readonly Gauge kReconnectionStatus = Metrics
                .CreateGauge("iiot_edge_reconnected", "reconnected count",
                    new GaugeConfiguration {
                        LabelNames = new[] { "module", "device", "timestamp_utc" }
                    });
            private static readonly Gauge kDisconnectionStatus = Metrics
                .CreateGauge("iiot_edge_disconnected", "reconnected count",
                    new GaugeConfiguration {
                        LabelNames = new[] { "module", "device", "timestamp_utc" }
                    });
        }


        /// <summary>
        /// Adapts device client to interface
        /// </summary>
        private sealed class DeviceClientAdapter : IIoTEdgeClient {

            /// <summary>
            /// Whether the client is closed
            /// </summary>
            public bool IsClosed { get; internal set; }

            /// <summary>
            /// Create client
            /// </summary>
            /// <param name="client"></param>
            internal DeviceClientAdapter(DeviceClient client) {
                _client = client ?? throw new ArgumentNullException(nameof(client));
            }

            /// <summary>
            /// Factory
            /// </summary>
            /// <param name="cs"></param>
            /// <param name="product"></param>
            /// <param name="deviceId"></param>
            /// <param name="transportSetting"></param>
            /// <param name="timeout"></param>
            /// <param name="onConnectionLost"></param>
            /// <param name="logger"></param>
            /// <returns></returns>
            public static async Task<IIoTEdgeClient> CreateAsync(string product,
                IotHubConnectionStringBuilder cs, string deviceId,
                ITransportSettings transportSetting, TimeSpan timeout,
                Action onConnectionLost, ILogger logger) {
                var client = Create(cs, transportSetting);
                var adapter = new DeviceClientAdapter(client);
                try {
                    // Configure
                    client.OperationTimeoutInMilliseconds = (uint)timeout.TotalMilliseconds;
                    client.SetConnectionStatusChangesHandler((s, r) =>
                        adapter.OnConnectionStatusChange(deviceId, onConnectionLost, logger, s, r));
                    client.ProductInfo = product;

                    await client.OpenAsync().ConfigureAwait(false);
                    return adapter;
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task CloseAsync() {
                if (IsClosed) {
                    return;
                }
                _client.OperationTimeoutInMilliseconds = 3000;
                _client.SetRetryPolicy(new NoRetry());
                IsClosed = true;
                await _client.CloseAsync().ConfigureAwait(false);
            }

            /// <inheritdoc />
            public async Task SendEventAsync(string route, Message message,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                if (message is null) {
                    throw new ArgumentNullException(nameof(message));
                }
                if (!string.IsNullOrEmpty(route)) {
                    message.Properties.Add(SystemProperties.To, route);
                }
                try {
                    await _client.SendEventAsync(message, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SendEventBatchAsync(string route, IEnumerable<Message> messages,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                if (!string.IsNullOrEmpty(route)) {
                    messages = messages.ToList();
                    foreach (var message in messages) {
                        message.Properties.Add(SystemProperties.To, route);
                    }
                }
                try {
                    await _client.SendEventBatchAsync(messages, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodHandlerAsync(string methodName,
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodHandlerAsync(methodName, methodHandler,
                        userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodDefaultHandlerAsync(
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodDefaultHandlerAsync(methodHandler,
                        userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetDesiredPropertyUpdateCallbackAsync(
                DesiredPropertyUpdateCallback callback, object userContext) {
                try {
                    await _client.SetDesiredPropertyUpdateCallbackAsync(callback,
                        userContext).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<Twin> GetTwinAsync(CancellationToken ct) {
                try {
                    return await _client.GetTwinAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties,
                CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                try {
                    await _client.UpdateReportedPropertiesAsync(reportedProperties, ct).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId,
                MethodRequest methodRequest, CancellationToken ct) {
                return Task.FromException<MethodResponse>(
                    new NotSupportedException("Device client does not support methods"));
            }

            /// <inheritdoc />
            public Task<MethodResponse> InvokeMethodAsync(string deviceId,
                MethodRequest methodRequest, CancellationToken ct) {
                return Task.FromException<MethodResponse>(
                    new NotSupportedException("Device client does not support methods"));
            }

            /// <inheritdoc />
            public void Dispose() {
                IsClosed = true;
                _client?.Dispose();
            }

            /// <summary>
            /// Handle status change event
            /// </summary>
            /// <param name="deviceId"></param>
            /// <param name="onConnectionLost"></param>
            /// <param name="logger"></param>
            /// <param name="status"></param>
            /// <param name="reason"></param>
            private void OnConnectionStatusChange(string deviceId,
                Action onConnectionLost, ILogger logger, ConnectionStatus status,
                ConnectionStatusChangeReason reason) {

                if (status == ConnectionStatus.Connected) {
                    logger.LogInformation("{counter}: Device {deviceId} reconnected " +
                        "due to {reason}.", _reconnectCounter, deviceId, reason);
                    kReconnectionStatus.WithLabels(deviceId,
                        DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
                        .Set(_reconnectCounter);
                    _reconnectCounter++;
                    return;
                }
                logger.LogInformation("{counter}: Device {deviceId} disconnected " +
                    "due to {reason} - now {status}...", _reconnectCounter, deviceId,
                        reason, status);
                kDisconnectionStatus.WithLabels(deviceId,
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
                    .Set(_reconnectCounter);
                if (IsClosed) {
                    // Already closed - nothing to do
                    return;
                }
                if (status == ConnectionStatus.Disconnected ||
                    status == ConnectionStatus.Disabled) {
                    // Force
                    IsClosed = true;
                    onConnectionLost?.Invoke();
                }
            }

            /// <summary>
            /// Helper to create device client
            /// </summary>
            /// <param name="cs"></param>
            /// <param name="transportSetting"></param>
            /// <returns></returns>
            private static Devices.Client.DeviceClient Create(IotHubConnectionStringBuilder cs,
                ITransportSettings transportSetting) {
                try {
                    if (cs == null) {
                        throw new ArgumentNullException(nameof(cs));
                    }
                    if (transportSetting != null) {
                        return DeviceClient.CreateFromConnectionString(cs.ToString(),
                            new ITransportSettings[] { transportSetting });
                    }
                    return DeviceClient.CreateFromConnectionString(cs.ToString());
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            private readonly DeviceClient _client;
            private int _reconnectCounter;
            private static readonly Gauge kReconnectionStatus = Metrics
                .CreateGauge("iiot_edge_device_reconnected", "reconnected count",
                    new GaugeConfiguration {
                        LabelNames = new[] { "device", "timestamp_utc" }
                    });
            private static readonly Gauge kDisconnectionStatus = Metrics
                .CreateGauge("iiot_edge_device_disconnected", "disconnected count",
                    new GaugeConfiguration {
                        LabelNames = new[] { "device", "timestamp_utc" }
                    });
        }

        /// <summary>
        /// Sdk logger event source hook
        /// </summary>
        internal sealed class IoTSdkLogger : EventSourceSink {

            /// <inheritdoc/>
            public IoTSdkLogger(ILogger logger) :
                base(logger) {
            }

            /// <inheritdoc/>
            public override void OnEvent(EventWrittenEventArgs eventData) {
                switch (eventData.EventName) {
                    case "Enter":
                    case "Exit":
                    case "Associate":
                        WriteEvent(LogLevel.Trace, eventData);
                        break;
                    default:
                        WriteEvent(LogLevel.Debug, eventData);
                        break;
                }
            }

            // ddbee999-a79e-5050-ea3c-6d1a8a7bafdd
            public const string EventSource = "Microsoft-Azure-Devices-Device-Client";
        }


        private readonly Task<IIoTEdgeClient> _client;
        private readonly ILogger _logger;
        private readonly IProcessControl _ctrl;
        private readonly IOptions<IoTEdgeClientOptions> _options;
        private readonly IIdentity _identity;
        private readonly IDisposable _logHook;
    }
}
