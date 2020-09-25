// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Diagnostics.Tracing;
    using Prometheus;

    /// <summary>
    /// Injectable IoT Sdk client
    /// </summary>
    public sealed class EdgeHubClient : IIoTEdgeClient, IIdentity, IDisposable {

        /// <inheritdoc />
        public string Hub { get; }

        /// <inheritdoc />
        public string DeviceId { get; }

        /// <inheritdoc />
        public string ModuleId { get; }

        /// <inheritdoc />
        public string Gateway { get; }

        /// <summary>
        /// Create sdk factory
        /// </summary>
        /// <param name="config"></param>
        /// <param name="broker"></param>
        /// <param name="ctrl"></param>
        /// <param name="logger"></param>
        public EdgeHubClient(IIoTEdgeClientConfig config, IEventSourceBroker broker,
            IProcessControl ctrl, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ctrl = ctrl ?? throw new ArgumentNullException(nameof(ctrl));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (broker != null) {
                _logHook = broker.Subscribe(IoTSdkLogger.EventSource, new IoTSdkLogger(logger));
            }

            // The runtime injects this as an environment variable
            var deviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            var moduleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");
            var gateway = Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME");
            var hub = gateway;

            IotHubConnectionStringBuilder cs = null;
            try {
                if (!string.IsNullOrEmpty(_config.EdgeHubConnectionString)) {
                    cs = IotHubConnectionStringBuilder.Create(_config.EdgeHubConnectionString);

                    if (string.IsNullOrEmpty(cs.SharedAccessKey)) {
                        throw new InvalidConfigurationException(
                            "Connection string is missing shared access key.");
                    }
                    if (string.IsNullOrEmpty(cs.DeviceId)) {
                        throw new InvalidConfigurationException(
                            "Connection string is missing device id.");
                    }


                    deviceId = cs.DeviceId;
                    moduleId = cs.ModuleId;
                    hub = cs.HostName;
                    gateway = cs.GatewayHostName ?? gateway;
                }
            }
            catch (Exception e) {
                _logger.Error(e,
                    "Bad configuration value in EdgeHubConnectionString config.");
            }

            Hub = hub;
            ModuleId = moduleId;
            DeviceId = deviceId;
            Gateway = gateway;

            if (string.IsNullOrEmpty(DeviceId)) {
                var ex = new InvalidConfigurationException(
        "If you are running outside of an IoT Edge context or in EdgeHubDev mode, then the " +
        "host configuration is incomplete and missing the EdgeHubConnectionString setting." +
        "You can run the module using the command line interface or in IoT Edge context, or " +
        "manually set the 'EdgeHubConnectionString' environment variable.");
                _logger.Error(ex, "The Twin module was not configured correctly.");
                throw ex;
            }

            var bypassCertValidation = _config.BypassCertVerification;
            if (!bypassCertValidation) {
                var certPath = Environment.GetEnvironmentVariable("EdgeModuleCACertificateFile");
                if (!string.IsNullOrWhiteSpace(certPath)) {
                    InstallCert(certPath);
                }
                else if (!string.IsNullOrEmpty(Gateway)) {
                    bypassCertValidation = true;
                }
            }

            TransportOption transportToUse;
            if (!string.IsNullOrEmpty(Gateway)) {
                //
                // Running in edge mode
                // the configured transport (if provided) will be forced to it's OverTcp
                // variant as follows: AmqpOverTcp when Amqp, AmqpOverWebsocket or
                // AmqpOverTcp specified and MqttOverTcp otherwise. Default is MqttOverTcp
                if ((_config.Transport & TransportOption.Mqtt) != 0) {
                    // prefer Mqtt over Amqp due to performance reasons
                    transportToUse = TransportOption.MqttOverTcp;
                }
                else {
                    transportToUse = TransportOption.AmqpOverTcp;
                }
                _logger.Information(
                    "Connecting all clients to {edgeHub} using {transport}.",
                        Gateway, transportToUse);
            }
            else {
                transportToUse = _config.Transport == 0 ?
                    TransportOption.Any : _config.Transport;
            }

            if (bypassCertValidation) {
                _logger.Warning("Bypassing certificate validation for client.");
            }
            var transportSettings = GetTransportSettings(bypassCertValidation,
                transportToUse);
            _client = CreateAdapterAsync(cs, transportSettings);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string route, Message message,
            CancellationToken ct) {
            var client = await _client;
            await client.SendEventAsync(route, message, ct);
        }

        /// <inheritdoc/>
        public async Task SendEventBatchAsync(string route,
            IEnumerable<Message> messages, CancellationToken ct) {
            var client = await _client;
            await client.SendEventBatchAsync(route, messages, ct);
        }

        /// <inheritdoc/>
        public async Task SetMethodDefaultHandlerAsync(
            MethodCallback methodHandler, object userContext) {
            var client = await _client;
            await client.SetMethodDefaultHandlerAsync(methodHandler, userContext);
        }

        /// <inheritdoc/>
        public async Task SetMethodHandlerAsync(string methodName,
            MethodCallback methodHandler, object userContext) {
            var client = await _client;
            await client.SetMethodHandlerAsync(methodName, methodHandler, userContext);
        }

        /// <inheritdoc/>
        public async Task<Twin> GetTwinAsync(CancellationToken ct) {
            var client = await _client;
            return await client.GetTwinAsync(ct);
        }

        /// <inheritdoc/>
        public async Task SetDesiredPropertyUpdateCallbackAsync(
            DesiredPropertyUpdateCallback callback, object userContext) {
            var client = await _client;
            await client.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);
        }

        /// <inheritdoc/>
        public async Task UpdateReportedPropertiesAsync(
            TwinCollection reportedProperties, CancellationToken ct) {
            var client = await _client;
            await client.UpdateReportedPropertiesAsync(reportedProperties, ct);
        }

        /// <inheritdoc/>
        public async Task<MethodResponse> InvokeMethodAsync(
            string deviceId, string moduleId,
            MethodRequest methodRequest, CancellationToken ct) {
            var client = await _client;
            return await client.InvokeMethodAsync(deviceId, moduleId, methodRequest, ct);
        }

        /// <inheritdoc/>
        public async Task<MethodResponse> InvokeMethodAsync(
            string deviceId, MethodRequest methodRequest, CancellationToken ct) {
            var client = await _client;
            return await client.InvokeMethodAsync(deviceId, methodRequest, ct);
        }

        /// <inheritdoc/>
        public async Task CloseAsync() {
            var client = await _client;
            if (client != null) {
                await client.CloseAsync();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => (_client?.Result as IDisposable)?.Dispose());
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
                        (sender, certificate, chain, sslPolicyErrors) => true;
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.MqttOverWebsocket) != 0) {
                var setting = new MqttTransportSettings(
                    TransportType.Mqtt_WebSocket_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.AmqpOverTcp) != 0) {
                var setting = new AmqpTransportSettings(
                    TransportType.Amqp_Tcp_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
                }
                transportSettings.Add(setting);
            }
            if ((transport & TransportOption.AmqpOverWebsocket) != 0) {
                var setting = new AmqpTransportSettings(
                    TransportType.Amqp_WebSocket_Only);
                if (bypassCertValidation) {
                    setting.RemoteCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
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
                    .ToArray());
            }
            return await CreateAdapterAsync(cs, (ITransportSettings)null);
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
            if (string.IsNullOrEmpty(ModuleId)) {
                if (cs == null) {
                    throw new InvalidConfigurationException(
                        "No connection string for device client specified.");
                }
                return await DeviceClientAdapter.CreateAsync(_config.Product, cs,
                    DeviceId, setting, timeout,
                        () => _ctrl?.Reset(), _logger);
            }
            return await ModuleClientAdapter.CreateAsync(_config.Product, cs,
                DeviceId, ModuleId, setting, timeout,
                    () => _ctrl?.Reset(), _logger);
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
                    logger.Information("Running in iotedge context.");
                }
                else {
                    logger.Information("Running outside iotedge context.");
                }

                var client = await CreateAsync(cs, transportSetting);
                var adapter = new ModuleClientAdapter(client);
                try {
                    // Configure
                    client.OperationTimeoutInMilliseconds = (uint)timeout.TotalMilliseconds;
                    client.SetConnectionStatusChangesHandler((s, r) =>
                        adapter.OnConnectionStatusChange(deviceId, moduleId, onConnectionLost,
                            logger, s, r));
                    client.ProductInfo = product;
                    await client.OpenAsync();
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
                    await _client.CloseAsync();
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SendEventAsync(string route, Message message, CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                try {
                    if (route != null) {
                        await _client.SendEventAsync(route, message, ct);
                    }
                    else {
                        await _client.SendEventAsync(message, ct);
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
                        await _client.SendEventBatchAsync(route, messages, ct);
                    }
                    else {
                        await _client.SendEventBatchAsync(messages, ct);
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
                    await _client.SetMethodHandlerAsync(methodName, methodHandler, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodDefaultHandlerAsync(
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodDefaultHandlerAsync(methodHandler, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetDesiredPropertyUpdateCallbackAsync(
                DesiredPropertyUpdateCallback callback, object userContext) {
                try {
                    await _client.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<Twin> GetTwinAsync(CancellationToken ct) {
                try {
                    return await _client.GetTwinAsync(ct);
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
                    await _client.UpdateReportedPropertiesAsync(reportedProperties, ct);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId,
                MethodRequest methodRequest, CancellationToken ct) {
                try {
                    return await _client.InvokeMethodAsync(deviceId, moduleId, methodRequest, ct);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<MethodResponse> InvokeMethodAsync(string deviceId,
                MethodRequest methodRequest, CancellationToken ct) {
                try {
                    return await _client.InvokeMethodAsync(deviceId, methodRequest, ct);
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
                    logger.Information("{counter}: Module {deviceId}_{moduleId} reconnected " +
                        "due to {reason}.", _reconnectCounter, deviceId, moduleId, reason);
                    kReconnectionStatus.WithLabels(moduleId, deviceId, DateTime.UtcNow.ToString())
                        .Set(_reconnectCounter);
                    _reconnectCounter++;
                    return;
                }
                kDisconnectionStatus.WithLabels(moduleId, deviceId, DateTime.UtcNow.ToString())
                    .Set(_reconnectCounter);
                logger.Information("{counter}: Module {deviceId}_{moduleId} disconnected " +
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
                            return await ModuleClient.CreateFromEnvironmentAsync();
                        }
                        return ModuleClient.CreateFromConnectionString(cs.ToString());
                    }
                    var ts = new ITransportSettings[] { transportSetting };
                    if (cs == null) {
                        return await ModuleClient.CreateFromEnvironmentAsync(ts);
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
        public sealed class DeviceClientAdapter : IIoTEdgeClient {

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

                    await client.OpenAsync();
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
                await _client.CloseAsync();
            }

            /// <inheritdoc />
            public async Task SendEventAsync(string route, Message message, CancellationToken ct) {
                if (IsClosed) {
                    return;
                }
                if (!string.IsNullOrEmpty(route)) {
                    message.Properties.Add(SystemProperties.To, route);
                }
                try {
                    await _client.SendEventAsync(message, ct);
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
                    await _client.SendEventBatchAsync(messages, ct);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodHandlerAsync(string methodName,
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodHandlerAsync(methodName, methodHandler, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetMethodDefaultHandlerAsync(
                MethodCallback methodHandler, object userContext) {
                try {
                    await _client.SetMethodDefaultHandlerAsync(methodHandler, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task SetDesiredPropertyUpdateCallbackAsync(
                DesiredPropertyUpdateCallback callback, object userContext) {
                try {
                    await _client.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            /// <inheritdoc />
            public async Task<Twin> GetTwinAsync(CancellationToken ct) {
                try {
                    return await _client.GetTwinAsync(ct);
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
                    await _client.UpdateReportedPropertiesAsync(reportedProperties, ct);
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
                    logger.Information("{counter}: Device {deviceId} reconnected " +
                        "due to {reason}.", _reconnectCounter, deviceId, reason);
                    kReconnectionStatus.WithLabels(deviceId, DateTime.UtcNow.ToString())
                        .Set(_reconnectCounter);
                    _reconnectCounter++;
                    return;
                }
                logger.Information("{counter}: Device {deviceId} disconnected " +
                    "due to {reason} - now {status}...", _reconnectCounter, deviceId,
                        reason, status);
                kDisconnectionStatus.WithLabels(deviceId, DateTime.UtcNow.ToString())
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
                        return Devices.Client.DeviceClient.CreateFromConnectionString(cs.ToString(),
                            new ITransportSettings[] { transportSetting });
                    }
                    return Devices.Client.DeviceClient.CreateFromConnectionString(cs.ToString());
                }
                catch (Exception ex) {
                    throw ex.Translate();
                }
            }

            private readonly Devices.Client.DeviceClient _client;
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
        /// Add certificate in local cert store for use by client for secure connection
        /// to iotedge runtime
        /// </summary>
        private void InstallCert(string certPath) {
            if (!File.Exists(certPath)) {
                // We cannot proceed further without a proper cert file
                _logger.Error("Missing certificate file: {certPath}", certPath);
                throw new InvalidOperationException("Missing certificate file.");
            }

            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            using (var cert = new X509Certificate2(X509Certificate.CreateFromCertFile(certPath))) {
                store.Add(cert);
            }
            _logger.Information("Added Cert: {certPath}", certPath);
            store.Close();
        }

        /// <summary>
        /// Sdk logger event source hook
        /// </summary>
        internal sealed class IoTSdkLogger : EventSourceSerilogSink {

            /// <inheritdoc/>
            public IoTSdkLogger(ILogger logger) :
                base(logger.ForContext("SourceContext", EventSource.Replace('-', '.'))) {
            }

            /// <inheritdoc/>
            public override void OnEvent(EventWrittenEventArgs eventData) {
                switch (eventData.EventName) {
                    case "Enter":
                    case "Exit":
                    case "Associate":
                        WriteEvent(LogEventLevel.Verbose, eventData);
                        break;
                    default:
                        WriteEvent(LogEventLevel.Debug, eventData);
                        break;
                }
            }

            // ddbee999-a79e-5050-ea3c-6d1a8a7bafdd
            public const string EventSource = "Microsoft-Azure-Devices-Device-Client";
        }


        private readonly Task<IIoTEdgeClient> _client;
        private readonly ILogger _logger;
        private readonly IProcessControl _ctrl;
        private readonly IIoTEdgeClientConfig _config;
        private readonly IDisposable _logHook;
    }
}
