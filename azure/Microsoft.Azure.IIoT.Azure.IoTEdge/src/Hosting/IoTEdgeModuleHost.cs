// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Hosting.Services;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using Prometheus;
    using System.Net;

    /// <summary>
    /// Module host implementation
    /// </summary>
    public sealed class IoTEdgeModuleHost : IModuleHost, ISettingsReporter,
        IIdentity, IEventClient, IJsonMethodClient {

        /// <inheritdoc/>
        public int MaxMethodPayloadCharacterCount => 120 * 1024;

        /// <inheritdoc/>
        public string DeviceId { get; private set; }

        /// <inheritdoc/>
        public string ModuleId { get; private set; }

        /// <inheritdoc/>
        public string Gateway { get; private set; }

        /// <summary>
        /// Create module host
        /// </summary>
        /// <param name="router"></param>
        /// <param name="settings"></param>
        /// <param name="factory"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgeModuleHost(IMethodRouter router, ISettingsRouter settings,
            IClientFactory factory, IJsonSerializer serializer, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            if (_client != null) {
                try {
                    await _lock.WaitAsync();
                    if (_client != null) {
                        _logger.Information("Stopping Module Host...");
                        try {
                            await _client.CloseAsync();
                        }
                        catch (OperationCanceledException) { }
                        catch (IotHubCommunicationException) { }
                        catch (DeviceNotFoundException) { }
                        catch (UnauthorizedException) { }
                        catch (Exception se) {
                            _logger.Error(se, "Module Host not cleanly disconnected.");
                        }
                    }
                    _logger.Information("Module Host stopped.");
                }
                catch (Exception ce) {
                    _logger.Error(ce, "Module Host stopping caused exception.");
                }
                finally {
                    kModuleStart.WithLabels(DeviceId ?? "", ModuleId ?? "", _moduleGuid, "",
                        DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
                        CultureInfo.InvariantCulture)).Set(0);
                    _client?.Dispose();
                    _client = null;
                    DeviceId = null;
                    ModuleId = null;
                    Gateway = null;
                    _lock.Release();
                }
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync(string type, string productInfo,
            string version, IProcessControl reset) {
            if (_client == null) {
                try {
                    await _lock.WaitAsync();
                    if (_client == null) {
                        // Create client
                        _logger.Debug("Starting Module Host...");
                        _client = await _factory.CreateAsync(productInfo + "_" + version, reset);
                        DeviceId = _factory.DeviceId;
                        ModuleId = _factory.ModuleId;
                        Gateway = _factory.Gateway;
                        // Register callback to be called when a method request is received
                        await _client.SetMethodDefaultHandlerAsync((request, _) =>
                            InvokeMethodAsync(request), null);

                        await InitializeTwinAsync();

                        // Register callback to be called when settings change ...
                        await _client.SetDesiredPropertyUpdateCallbackAsync(
                            (settings, _) => ProcessSettingsAsync(settings), null);

                        // Report type of service, chosen site, and connection state
                        var twinSettings = new TwinCollection {
                            [TwinProperty.Type] = type
                        };

                        // Set version information
                        twinSettings[TwinProperty.Version] = version;
                        await _client.UpdateReportedPropertiesAsync(twinSettings);

                        // Done...
                        kModuleStart.WithLabels(DeviceId ?? "", ModuleId ?? "",
                            _moduleGuid, version,
                            DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
                            CultureInfo.InvariantCulture)).Set(1);
                        _logger.Information("Module Host started.");
                        return;
                    }
                }
                catch (Exception ex) {
                    kModuleStart.WithLabels(DeviceId ?? "", ModuleId ?? "",
                        _moduleGuid, version,
                        DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
                        CultureInfo.InvariantCulture)).Set(0);
                    _logger.Error("Module Host failed to start.");
                    _client?.Dispose();
                    _client = null;
                    DeviceId = null;
                    ModuleId = null;
                    Gateway = null;
                    throw ex;
                }
                finally {
                    _lock.Release();
                }
            }
            throw new InvalidOperationException("Already started");
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            try {
                await _lock.WaitAsync();
                if (_client != null) {
                    var messages = batch
                        .Select(ev =>
                             CreateMessage(ev, contentEncoding, contentType, eventSchema,
                                DeviceId, ModuleId))
                        .ToList();
                    try {
                        await _client.SendEventBatchAsync(target, messages);
                    }
                    finally {
                        messages.ForEach(m => m?.Dispose());
                    }
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] data, string contentType, string eventSchema,
            string contentEncoding, CancellationToken ct) {
            try {
                await _lock.WaitAsync();
                if (_client != null) {
                    using (var msg = CreateMessage(data, contentEncoding, contentType,
                        eventSchema, DeviceId, ModuleId)) {
                        await _client.SendEventAsync(target, msg, ct);
                    }
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ReportAsync(IEnumerable<KeyValuePair<string, VariantValue>> properties,
            CancellationToken ct) {
            try {
                await _lock.WaitAsync();
                if (_client != null) {
                    var collection = new TwinCollection();
                    foreach (var property in properties) {
                        collection[property.Key] = property.Value?.ConvertTo<object>();
                    }
                    await _client.UpdateReportedPropertiesAsync(collection, ct);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ReportAsync(string propertyId, VariantValue value, CancellationToken ct) {
            try {
                await _lock.WaitAsync();
                if (_client != null) {
                    var collection = new TwinCollection {
                        [propertyId] = value?.ConvertTo<object>()
                    };
                    await _client.UpdateReportedPropertiesAsync(collection, ct);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<string> CallMethodAsync(string deviceId, string moduleId,
            string method, string payload, TimeSpan? timeout, CancellationToken ct) {
            var request = new MethodRequest(method, Encoding.UTF8.GetBytes(payload),
                timeout, null);
            MethodResponse response;
            if (string.IsNullOrEmpty(moduleId)) {
                response = await _client.InvokeMethodAsync(deviceId, request, ct);
            }
            else {
                response = await _client.InvokeMethodAsync(deviceId, moduleId, request, ct);
            }
            if (response.Status != 200) {
                throw new MethodCallStatusException(
                    response.ResultAsJson, response.Status);
            }
            return response.ResultAsJson;
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_client != null) {
                StopAsync().Wait();
            }
            _lock.Dispose();
        }

        /// <summary>
        /// Create message
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        private static Message CreateMessage(byte[] data, string contentEncoding,
            string contentType, string eventSchema, string deviceId, string moduleId) {
            var msg = new Message(data) {

                ContentType = contentType,
                ContentEncoding = contentEncoding,
                // TODO - setting CreationTime causes issues in the Azure IoT java SDK
                // revert the comment when the issue is fixed
                //  CreationTimeUtc = DateTime.UtcNow
            };
            if (!string.IsNullOrEmpty(contentEncoding)) {
                msg.Properties.Add(CommonProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                msg.Properties.Add(CommonProperties.EventSchemaType, eventSchema);
            }
            if (!string.IsNullOrEmpty(deviceId)) {
                msg.Properties.Add(CommonProperties.DeviceId, deviceId);
            }
            if (!string.IsNullOrEmpty(moduleId)) {
                msg.Properties.Add(CommonProperties.ModuleId, moduleId);
            }
            return msg;
        }

        /// <summary>
        /// Invoke method handler on method router
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<MethodResponse> InvokeMethodAsync(MethodRequest request) {
            const int kMaxMessageSize = 127 * 1024;
            try {
                var result = await _router.InvokeAsync(request.Name, request.Data,
                    ContentMimeType.Json);
                if (result.Length > kMaxMessageSize) {
                    _logger.Error("Result (Payload too large => {Length}", result.Length);
                    return new MethodResponse((int)HttpStatusCode.RequestEntityTooLarge);
                }
                return new MethodResponse(result, 200);
            }
            catch (MethodCallStatusException mex) {
                var payload = Encoding.UTF8.GetBytes(mex.ResponsePayload);
                return new MethodResponse(payload.Length > kMaxMessageSize ? null : payload,
                    mex.Result);
            }
            catch (Exception) {
                return new MethodResponse((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Reads the twin including desired and reported settings and applies them to the
        /// settings controllers.  updates the twin for any changes resulting from the
        /// update.  Reported values are cached until user calls Refresh.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeTwinAsync() {

            System.Diagnostics.Debug.Assert(_lock.CurrentCount == 0);

            // Process initial setting snapshot from twin
            var twin = await _client.GetTwinAsync();
            if (!string.IsNullOrEmpty(twin.DeviceId)) {
                DeviceId = twin.DeviceId;
            }
            if (!string.IsNullOrEmpty(twin.ModuleId)) {
                ModuleId = twin.ModuleId;
            }
            _logger.Information("Initialize device twin for {deviceId} - {moduleId}",
                DeviceId, ModuleId ?? "standalone");

            var desired = new Dictionary<string, VariantValue>();
            var reported = new Dictionary<string, VariantValue>();

            // Start with reported values which we desire to be re-applied
            foreach (KeyValuePair<string, dynamic> property in twin.Properties.Reported) {
                var value = (VariantValue)_serializer.FromObject(property.Value);
                if (value.IsObject &&
                    value.TryGetProperty("status", out _) &&
                    value.PropertyNames.Count() == 1) {
                    // Clear status properties from twin
                    continue;
                }
            }
            // Apply desired values on top.
            foreach (KeyValuePair<string, dynamic> property in twin.Properties.Desired) {
                var value = (VariantValue)_serializer.FromObject(property.Value);
                if (!ProcessEdgeHostSettings(property.Key, value, reported)) {
                    desired[property.Key] = value;
                }
            }

            // Process settings on controllers
            _logger.Information("Applying initial desired state.");
            await _settings.ProcessSettingsAsync(desired);

            // Synchronize all controllers with reported
            _logger.Information("Reporting currently initial state.");
            await ReportControllerStateAsync(twin, reported);
        }

        /// <summary>
        /// Synchronize controllers with current reported twin state
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="reported"></param>
        /// <returns></returns>
        private async Task ReportControllerStateAsync(Twin twin,
            Dictionary<string, VariantValue> reported) {
            var processed = await _settings.GetSettingsStateAsync();

            // If there are changes, update what should be reported back.
            foreach (var property in processed) {
                var exists = twin.Properties.Reported.Contains(property.Key);
                if (VariantValueEx.IsNull(property.Value)) {
                    if (exists) {
                        // If exists as reported, remove
                        reported.AddOrUpdate(property.Key, null);
                    }
                }
                else {
                    if (exists) {
                        // If exists and same as property value, continue
                        var r = (VariantValue)_serializer.FromObject(
                            twin.Properties.Reported[property.Key]);
                        if (r == property.Value) {
                            continue;
                        }
                    }
                    else if (VariantValueEx.IsNull(property.Value)) {
                        continue;
                    }

                    // Otherwise, add to reported properties
                    reported[property.Key] = property.Value;
                }
            }
            if (reported.Count > 0) {
                _logger.Debug("Reporting controller state...");
                var collection = new TwinCollection();
                foreach (var item in reported) {
                    collection[item.Key] = item.Value?.ConvertTo<object>();
                }
                await _client.UpdateReportedPropertiesAsync(collection);
                _logger.Debug("Complete controller state reported (properties: {@settings}).",
                    reported.Keys);
            }
        }

        /// <summary>
        /// Update device client settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private async Task ProcessSettingsAsync(TwinCollection settings) {
            if (settings.Count > 0) {
                try {
                    await _lock.WaitAsync();

                    // Patch existing reported properties
                    var desired = new Dictionary<string, VariantValue>();
                    var reporting = new Dictionary<string, VariantValue>();

                    foreach (KeyValuePair<string, dynamic> property in settings) {
                        var value = (VariantValue)_serializer.FromObject(property.Value);
                        if (!ProcessEdgeHostSettings(property.Key, value, reporting)) {
                            desired.AddOrUpdate(property.Key, value);
                        }
                    }

                    if (reporting != null && reporting.Count != 0) {
                        var collection = new TwinCollection();
                        foreach (var item in reporting) {
                            collection[item.Key] = item.Value?.ConvertTo<object>();
                        }
                        await _client.UpdateReportedPropertiesAsync(collection);
                        _logger.Debug("Internal state updated...", reporting);
                    }

                    // Any controller properties left?
                    if (desired.Count == 0) {
                        return;
                    }

                    _logger.Debug("Processing new settings...");
                    var reported = await _settings.ProcessSettingsAsync(desired);

                    if (reported != null && reported.Count != 0) {
                        _logger.Debug("Reporting setting results...");
                        var collection = new TwinCollection();
                        foreach (var item in reported) {
                            collection[item.Key] = item.Value?.ConvertTo<object>();
                        }
                        await _client.UpdateReportedPropertiesAsync(collection);
                    }
                    _logger.Information("New settings processed.");
                }
                finally {
                    _lock.Release();
                }
            }
        }

        /// <summary>
        /// Process default settings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="processed"></param>
        /// <returns></returns>
        private bool ProcessEdgeHostSettings(string key, VariantValue value,
            IDictionary<string, VariantValue> processed = null) {
            switch (key.ToLowerInvariant()) {
                case TwinProperty.Version:
                case TwinProperty.SiteId:
                case TwinProperty.Type:
                    break;
                default:
                    return false;
            }
            if (processed != null) {
                processed[key] = value;
            }
            return true;
        }

        private IClient _client;

        private readonly IMethodRouter _router;
        private readonly ISettingsRouter _settings;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IClientFactory _factory;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly string _moduleGuid = Guid.NewGuid().ToString();

        private static readonly Gauge kModuleStart = Metrics
            .CreateGauge("iiot_edge_module_start", "starting module",
                new GaugeConfiguration {
                    LabelNames = new[] {"deviceid", "module", "runid", "version", "timestamp_utc" }
                });
    }
}
