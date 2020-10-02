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
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Prometheus;

    /// <summary>
    /// Module host implementation
    /// </summary>
    public sealed class IoTEdgeModuleHost : IModuleHost, ISettingsReporter {

        /// <summary>
        /// Create module host
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgeModuleHost(ISettingsRouter settings, IIoTEdgeClient client,
            IJsonSerializer serializer, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task StartAsync(string type, string version) {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_started) {
                    throw new InvalidOperationException("Already started");
                }

                _logger.Debug("Starting Module Host...");
                await InitializeTwinAsync().ConfigureAwait(false);

                // Register callback to be called when settings change ...
                await _client.SetDesiredPropertyUpdateCallbackAsync(
                    (settings, _) => ProcessSettingsAsync(settings), null).ConfigureAwait(false);

                // Report type of service, chosen site, and connection state
                var twinSettings = new TwinCollection {
                    [TwinProperty.Type] = type
                };

                // Set version information
                twinSettings[TwinProperty.Version] = version;
                await _client.UpdateReportedPropertiesAsync(twinSettings).ConfigureAwait(false);

                _logger.Information("Module Host started.");
                _started = true;
            }
            catch (Exception ex) {
                _logger.Error(ex, "Module Host failed to start.");
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_started) {
                    await _client.SetDesiredPropertyUpdateCallbackAsync(
                        null, null).ConfigureAwait(false);
                }
            }
            finally {
                _started = false;
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ReportAsync(IEnumerable<KeyValuePair<string, VariantValue>> properties,
            CancellationToken ct) {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_client != null) {
                    var collection = new TwinCollection();
                    foreach (var property in properties) {
                        collection[property.Key] = property.Value?.ConvertTo<object>();
                    }
                    await _client.UpdateReportedPropertiesAsync(collection, ct).ConfigureAwait(false);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ReportAsync(string propertyId, VariantValue value, CancellationToken ct) {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_client != null) {
                    var collection = new TwinCollection {
                        [propertyId] = value?.ConvertTo<object>()
                    };
                    await _client.UpdateReportedPropertiesAsync(collection, ct).ConfigureAwait(false);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_client != null) {
                StopAsync().Wait();
            }
            _lock.Dispose();
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
            var twin = await _client.GetTwinAsync().ConfigureAwait(false);
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
            await _settings.ProcessSettingsAsync(desired).ConfigureAwait(false);

            // Synchronize all controllers with reported
            _logger.Information("Reporting currently initial state.");
            await ReportControllerStateAsync(twin, reported).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronize controllers with current reported twin state
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="reported"></param>
        /// <returns></returns>
        private async Task ReportControllerStateAsync(Twin twin,
            Dictionary<string, VariantValue> reported) {
            var processed = await _settings.GetSettingsStateAsync().ConfigureAwait(false);

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
                await _client.UpdateReportedPropertiesAsync(collection).ConfigureAwait(false);
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
                    await _lock.WaitAsync().ConfigureAwait(false);

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
                        await _client.UpdateReportedPropertiesAsync(collection).ConfigureAwait(false);
                        _logger.Debug("Internal state updated...", reporting);
                    }

                    // Any controller properties left?
                    if (desired.Count == 0) {
                        return;
                    }

                    _logger.Debug("Processing new settings...");
                    var reported = await _settings.ProcessSettingsAsync(desired).ConfigureAwait(false);

                    if (reported != null && reported.Count != 0) {
                        _logger.Debug("Reporting setting results...");
                        var collection = new TwinCollection();
                        foreach (var item in reported) {
                            collection[item.Key] = item.Value?.ConvertTo<object>();
                        }
                        await _client.UpdateReportedPropertiesAsync(collection).ConfigureAwait(false);
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

        private bool _started;
        private readonly IIoTEdgeClient _client;
        private readonly ISettingsRouter _settings;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly string _moduleGuid = Guid.NewGuid().ToString();

        private static readonly Gauge kModuleStart = Metrics
            .CreateGauge("iiot_edge_module_start", "starting module",
                new GaugeConfiguration {
                    LabelNames = new[] { "deviceid", "module", "runid", "version", "timestamp_utc" }
                });
    }
}
