// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Clients {
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of twin services using service sdk.
    /// </summary>
    public sealed class IoTHubServiceClient : IDeviceTwinServices {

        /// <summary>
        /// The host name the client is talking to
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTHubServiceClient(IOptions<IoTHubOptions> options, IJsonSerializer serializer,
            ILogger logger) {
            if (string.IsNullOrEmpty(options.Value.IoTHubConnString)) {
                throw new ArgumentException("Missing connection string", nameof(options));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = ServiceClient.CreateFromConnectionString(options.Value.IoTHubConnString);
            _registry = RegistryManager.CreateFromConnectionString(options.Value.IoTHubConnString);

            Task.WaitAll(_client.OpenAsync(), _registry.OpenAsync());

            HostName = ConnectionString.Parse(options.Value.IoTHubConnString).HostName;
        }

        /// <inheritdoc/>
        public async Task<DeviceTwinModel> RegisterAsync(DeviceRegistrationModel registration,
            bool forceUpdate, CancellationToken ct) {

            // First try create device
            try {
                var device = await _registry.AddDeviceAsync(registration.ToDevice(),
                    ct).ConfigureAwait(false);
            }
            catch (DeviceAlreadyExistsException)
                when (!string.IsNullOrEmpty(registration.ModuleId) || forceUpdate) {
                // continue
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Create device failed during registration");
                throw e.Translate();
            }

            // Then update twin assuming it now exists. If fails, retry...
            if (!string.IsNullOrEmpty(registration.ModuleId)) {
                // Try create module
                try {
                    var module = await _registry.AddModuleAsync(registration.ToModule(),
                        ct).ConfigureAwait(false);
                }
                catch (DeviceAlreadyExistsException) when (forceUpdate) {
                    // Expected for update
                }
                catch (Exception e) {
                    _logger.LogTrace(e, "Create module failed during registration");
                    throw e.Translate();
                }
            }
            try {
                Twin update;
                // Then update twin assuming it now exists. If fails, retry...
                var etag = "*";
                if (!string.IsNullOrEmpty(registration.ModuleId)) {
                    update = await _registry.UpdateTwinAsync(registration.Id,
                        registration.ModuleId, registration.ToTwin(), etag,
                            ct).ConfigureAwait(false);
                }
                else {
                    // Patch device
                    update = await _registry.UpdateTwinAsync(registration.Id,
                        registration.ToTwin(), etag, ct).ConfigureAwait(false);
                }
                return _serializer.DeserializeTwin(update, HostName);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Registration failed");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceTwinModel> PatchAsync(DeviceTwinModel twin,
            bool force, CancellationToken ct) {
            try {
                Twin update;
                // Then update twin assuming it now exists. If fails, retry...
                var etag = string.IsNullOrEmpty(twin.Etag) || force ? "*" : twin.Etag;
                if (!string.IsNullOrEmpty(twin.ModuleId)) {
                    update = await _registry.UpdateTwinAsync(twin.Id, twin.ModuleId,
                        twin.ToTwin(true), etag, ct).ConfigureAwait(false);
                }
                else {
                    // Patch device
                    update = await _registry.UpdateTwinAsync(twin.Id,
                        twin.ToTwin(true), etag, ct).ConfigureAwait(false);
                }
                return _serializer.DeserializeTwin(update, HostName);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Create or update failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceTwinModel> GetAsync(string deviceId, string moduleId,
            CancellationToken ct) {
            try {
                Twin twin = null;
                if (string.IsNullOrEmpty(moduleId)) {
                    twin = await _registry.GetTwinAsync(deviceId, ct).ConfigureAwait(false);
                }
                else {
                    twin = await _registry.GetTwinAsync(deviceId, moduleId, ct).ConfigureAwait(false);
                }
                return _serializer.DeserializeTwin(twin, HostName);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Get twin failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceModel> GetRegistrationAsync(string deviceId, string moduleId,
            CancellationToken ct) {
            try {
                if (string.IsNullOrEmpty(moduleId)) {
                    var device = await _registry.GetDeviceAsync(deviceId, ct).ConfigureAwait(false);
                    return device.ToModel(HostName);
                }
                var module = await _registry.GetModuleAsync(deviceId, moduleId, ct).ConfigureAwait(false);
                return module.ToModel(HostName);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Get registration failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<QueryResultModel> QueryAsync(string query, string continuation,
            int? pageSize, CancellationToken ct) {
            try {
                if (continuation != null) {
                    _serializer.DeserializeContinuationToken(continuation,
                        out query, out continuation, out pageSize);
                }
                var options = new QueryOptions { ContinuationToken = continuation };
                var statement = _registry.CreateQuery(query, pageSize);
                var result = await statement.GetNextAsJsonAsync(options).ConfigureAwait(false);
                return new QueryResultModel {
                    ContinuationToken = _serializer.SerializeContinuationToken(query,
                        result.ContinuationToken, pageSize),
                    Result = result.Select(s => _serializer.Parse(s))
                };
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Query failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task UpdatePropertiesAsync(string deviceId, string moduleId,
            Dictionary<string, VariantValue> properties, string etag, CancellationToken ct) {
            try {
                var result = await (string.IsNullOrEmpty(moduleId) ?
                    _registry.UpdateTwinAsync(deviceId, properties.ToTwin(), etag, ct) :
                    _registry.UpdateTwinAsync(deviceId, moduleId, properties.ToTwin(), etag, ct)).ConfigureAwait(false);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Update properties failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string deviceId, string moduleId, string etag,
            CancellationToken ct) {
            try {
                await (string.IsNullOrEmpty(moduleId) ?
                    _registry.RemoveDeviceAsync(new Device(deviceId) {
                        ETag = etag ?? "*"
                    }, ct) :
                    _registry.RemoveModuleAsync(new Module(deviceId, moduleId) {
                        ETag = etag ?? "*"
                    }, ct)).ConfigureAwait(false);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Delete failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<MethodResultModel> CallMethodAsync(string deviceId, string moduleId,
            MethodParameterModel parameters, CancellationToken ct) {
            try {
                var methodInfo = new CloudToDeviceMethod(parameters.Name);
                methodInfo.SetPayloadJson(parameters.JsonPayload);
                var result = await (string.IsNullOrEmpty(moduleId) ?
                     _client.InvokeDeviceMethodAsync(deviceId, methodInfo, ct) :
                     _client.InvokeDeviceMethodAsync(deviceId, moduleId, methodInfo, ct)).ConfigureAwait(false);
                return new MethodResultModel {
                    JsonPayload = result.GetPayloadAsJson(),
                    Status = result.Status
                };
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Call method failed ");
                throw e.Translate();
            }
        }

        private readonly ServiceClient _client;
        private readonly RegistryManager _registry;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
    }
}
