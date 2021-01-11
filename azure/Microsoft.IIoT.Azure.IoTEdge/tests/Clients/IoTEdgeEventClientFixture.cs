// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public sealed class IoTEdgeEventClientFixture {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        internal IoTEdgeEventClientHarness GetHarness(string resource) {
            return new IoTEdgeEventClientHarness(resource, IoTHubServiceFixture.Up && !Skip);
        }
    }

    internal sealed class IoTEdgeEventClientHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        internal IoTEdgeEventClientHarness(string resource, bool serviceUp) {
            if (!serviceUp) {
                _container = null;
                _service = null;
                return;
            }
            try {
                // Read connections string from keyvault
                var config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();

                var builder = new ContainerBuilder();
                builder.AddConfiguration(config);
                builder.RegisterModule<NewtonSoftJsonModule>();
                builder.RegisterModule<IoTHubSupportModule>();
                builder.AddDiagnostics();
                _service = builder.Build();

                // Create edge resource and get connection string for it
                var deviceId = HubResource.Parse(resource, out _, out var moduleId);
                var connectionString = CreateResourceAsync(resource).Result;
                resource = HubResource.Format(connectionString.HostName, deviceId, moduleId);

                // Create edge hosting container
                builder = new ContainerBuilder();
                builder.AddConfiguration(config);
                builder.RegisterModule<NewtonSoftJsonModule>();

                builder.RegisterModule<IoTEdgeHosting>();
                builder.Configure<IoTEdgeClientOptions>(options => {
                    options.EdgeHubConnectionString = connectionString.ToString();
                });

                // Test specific Event processing
                builder.RegisterModule<IoTHubEventsModule>();
                builder.RegisterInstance(new TestHandler(this, "Test1"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new TestHandler(this, "Test2"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new TestHandler(this, "Test3"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new UnknownHandler(this))
                    .AsImplementedInterfaces();

                builder.AddDiagnostics();
                _container = builder.Build();

                _resource = resource;
                Try.Op(() => IoTHubServiceFixture.Register(_resource,
                    _container.Resolve<IEventConsumer>()));
            }
            catch {
                DeleteResourceAsync(resource).Wait();
                _service?.Dispose();
                _service = null;

                _container?.Dispose();
                _container = null;
            }
        }

        /// <summary>
        /// Get Event client
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventClient GetEventClient() {
            return Try.Op(() => _container?.Resolve<IEventClient>());
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_container != null) {
                Try.Op(() => IoTHubServiceFixture.Unregister(_resource));
                _container.Dispose();
            }
            if (_service != null) {
                DeleteResourceAsync(_resource).Wait();
                _service.Dispose();
            }
        }

        /// <summary>
        /// Create device or module resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal async Task<ConnectionString> CreateResourceAsync(string resource) {
            var registry = _service.Resolve<IDeviceTwinServices>();
            var deviceId = HubResource.Parse(resource, out _, out var moduleId);
            await registry.RegisterAsync(deviceId, moduleId, new DeviceRegistrationModel {
                Tags = new Dictionary<string, VariantValue> {
                    [TwinProperty.Type] = "test"
                }
            }).ConfigureAwait(false);
            return await registry.GetConnectionStringAsync(
                deviceId, moduleId).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete device or module resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal async Task DeleteResourceAsync(string resource) {
            var registry = _service.Resolve<IDeviceTwinServices>();
            var deviceId = HubResource.Parse(resource, out _, out var moduleId);
            await registry.DeleteAsync(deviceId, moduleId).ConfigureAwait(false);
        }

        internal class TestHandler : ITelemetryHandler {

            public TestHandler(IoTEdgeEventClientHarness outer, string schema) {
                _outer = outer;
                MessageSchema = schema;
            }

            public string MessageSchema { get; }

            public Task HandleAsync(string source,
                byte[] payload, IEventProperties properties,
                Func<Task> checkpoint) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    MessageSchema, source, payload, properties));
                return Task.CompletedTask;
            }

            public Task OnBatchCompleteAsync() {
                _outer.OnComplete?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }

            private readonly IoTEdgeEventClientHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(IoTEdgeEventClientHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IEventProperties properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly IoTEdgeEventClientHarness _outer;
        }

        private readonly IContainer _container;
        private readonly IContainer _service;
        private readonly string _resource;
    }

    internal class TelemetryEventArgs : EventArgs {

        internal TelemetryEventArgs(string schema, string source,
            byte[] data, IEventProperties properties) {
            HandlerSchema = schema;
            Source = source;
            if (source != null) {
                try {
                    DeviceId = HubResource.Parse(source, out var hub, out var moduleId);
                    Hub = hub;
                    ModuleId = moduleId;
                }
                catch {

                }
            }
            Data = data;
            Target = properties.TryGetValue(EventProperties.Target, out var v) ? v : null;
            Properties = properties
                .Where(k => k.Key != EventProperties.Target)
                .Where(k => !k.Key.StartsWith("x-", StringComparison.Ordinal))
                .ToEventProperties();
        }

        public string Target { get; }
        public string Source { get; }
        public string HandlerSchema { get; }
        public string Hub { get; }
        public string DeviceId { get; }
        public string ModuleId { get; }
        public byte[] Data { get; }
        public IEventProperties Properties { get; }
    }

    internal delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}