// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Mock {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A mocked iot sdk client
    /// </summary>
    public class IoTHubClient : IIoTEdgeClient, IIdentity, IDisposable,
        IIoTClientCallback {

        /// <inheritdoc />
        public string Hub { get; }

        /// <inheritdoc />
        public string DeviceId { get; }

        /// <inheritdoc />
        public string ModuleId { get; }

        /// <inheritdoc />
        public string Gateway { get; }

        /// <summary>
        /// Whether the client is closed
        /// </summary>
        public bool IsClosed { get; internal set; }

        /// <summary>
        /// Connection to iot hub
        /// </summary>
        public IIoTHubConnection Connection { get; internal set; }

        /// <summary>
        /// Create sdk factory
        /// </summary>
        /// <param name="hub">Outer hub abstraction</param>
        /// <param name="config">Module framework configuration</param>
        /// <param name="ctrl">Process control</param>
        public IoTHubClient(IIoTHub hub, IIoTEdgeClientConfig config,
            IProcessControl ctrl = null) {
            _ctrl = ctrl;
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            if (string.IsNullOrEmpty(config.EdgeHubConnectionString)) {
                throw new InvalidConfigurationException(
                    "Must have connection string or module id to create clients.");
            }
            var cs = IotHubConnectionStringBuilder.Create(config.EdgeHubConnectionString);
            if (string.IsNullOrEmpty(cs.DeviceId)) {
                throw new InvalidConfigurationException(
                    "Connection string is not a device or module connection string.");
            }

            Hub = hub.HostName;
            DeviceId = cs.DeviceId;
            ModuleId = cs.ModuleId;
            Gateway = cs.GatewayHostName;

            Connection = _hub.Connect(DeviceId, ModuleId, this);
            if (Connection == null) {
                throw new CommunicationException("Failed to connect to fake hub");
            }
        }

        /// <inheritdoc />
        public Task CloseAsync() {
            Connection.Close();
            Connection = null;
            IsClosed = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendEventAsync(string target, Message message, CancellationToken ct) {
            // Add event to telemetry list
            if (!IsClosed) {
                Connection.SendEvent(message);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendEventBatchAsync(string target, IEnumerable<Message> messages,
            CancellationToken ct) {
            if (!IsClosed) {
                foreach (var message in messages) {
                    Connection.SendEvent(message);
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetMethodHandlerAsync(string methodName,
            MethodCallback methodHandler, object userContext) {
            if (!IsClosed) {
                _methods.AddOrUpdate(methodName, (methodHandler, userContext));
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetMethodDefaultHandlerAsync(
            MethodCallback methodHandler, object userContext) {
            if (!IsClosed) {
                _methods.AddOrUpdate("$default", (methodHandler, userContext));
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetDesiredPropertyUpdateCallbackAsync(
            DesiredPropertyUpdateCallback callback, object userContext) {
            if (!IsClosed) {
                _properties.Add((callback, userContext));
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<Twin> GetTwinAsync(CancellationToken ct) {
            return Task.FromResult(IsClosed ? null : Connection.GetTwin());
        }

        /// <inheritdoc />
        public Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties,
            CancellationToken ct) {
            if (!IsClosed) {
                Connection.UpdateReportedProperties(reportedProperties);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId,
            MethodRequest methodRequest, CancellationToken cancellationToken) {
            return Task.FromResult(IsClosed ? null :
                Connection.Call(deviceId, moduleId, methodRequest));
        }

        /// <inheritdoc />
        public Task<MethodResponse> InvokeMethodAsync(string deviceId,
            MethodRequest methodRequest, CancellationToken cancellationToken) {
            return Task.FromResult(IsClosed ? null :
                Connection.Call(deviceId, null, methodRequest));
        }

        /// <inheritdoc />
        public void SetDesiredProperties(TwinCollection desiredProperties) {
            foreach (var (cb, ctx) in _properties) {
                cb(desiredProperties, ctx);
            }
        }

        /// <inheritdoc />
        public MethodResponse Call(MethodRequest methodRequest) {
            if (!_methods.TryGetValue(methodRequest.Name, out var item)) {
                if (!_methods.TryGetValue("$default", out item)) {
                    return new MethodResponse(500);
                }
            }
            try {
                var (cb, ctx) = item;
                return cb(methodRequest, ctx).Result;
            }
            catch {
                return new MethodResponse(500);
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            CloseAsync().Wait();
        }

        /// <inheritdoc />
        public void RemoteDisconnect() {
            Connection = null;
            IsClosed = true;
        }

        private readonly Dictionary<string, (MethodCallback, object)> _methods =
            new Dictionary<string, (MethodCallback, object)>();
        private readonly List<(DesiredPropertyUpdateCallback, object)> _properties =
            new List<(DesiredPropertyUpdateCallback, object)>();
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IProcessControl _ctrl;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly IIoTHub _hub;
    }
}
