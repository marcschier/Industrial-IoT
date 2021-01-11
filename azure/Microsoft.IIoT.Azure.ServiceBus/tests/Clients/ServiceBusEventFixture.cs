// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.IIoT.Extensions.Messaging.Handlers;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public sealed class ServiceBusEventFixture : IDisposable {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        internal ServiceBusEventQueueHarness GetHarness(string queue) {
            if (Skip) {
                return new ServiceBusEventQueueHarness(null);
            }
            return new ServiceBusEventQueueHarness(queue);
        }

        /// <inheritdoc/>
        public void Dispose() {
        }
    }

    internal sealed class ServiceBusEventQueueHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        internal ServiceBusEventQueueHarness(string queue) {
            try {
                var builder = new ContainerBuilder();

                // Read connections string from keyvault
                var config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();
                builder.AddConfiguration(config);
                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                // Set queue name
                builder.Configure<ServiceBusProcessorOptions>(options => options.Queue = queue);

                builder.RegisterModule<ServiceBusEventQueueSupport>();
                builder.RegisterModule<ServiceBusEventProcessorSupport>();
                builder.RegisterModule<NewtonSoftJsonModule>();

                builder.RegisterType<DeviceEventHandler>()
                    .AsImplementedInterfaces().InstancePerDependency();

                builder.RegisterInstance(new TestHandler(this, "Test1"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new TestHandler(this, "Test2"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new TestHandler(this, "Test3"))
                    .AsImplementedInterfaces();
                builder.RegisterInstance(new UnknownHandler(this))
                    .AsImplementedInterfaces();

                builder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();

                builder.AddDiagnostics();
                _container = builder.Build();
                _queue = queue;
            }
            catch {
                _container = null;
            }
        }

        /// <summary>
        /// Get Event Queue client
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventPublisherClient GetEventPublisherClient() {
            return Try.Op(() => _container?.Resolve<IEventPublisherClient>());
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
            if (_container == null) {
                return;
            }
            var config = _container.Resolve<IOptions<ServiceBusOptions>>();
            var managementClient = new ManagementClient(config.Value.ConnectionString);
            //  managementClient.GetQueuesAsync().Result
            //      .ToList().ForEach(q => managementClient.DeleteQueueAsync(q.Path).Wait());
            managementClient.DeleteQueueAsync(_queue).Wait();
            _container.Dispose();
        }

        internal class TestHandler : ITelemetryHandler {

            public TestHandler(ServiceBusEventQueueHarness outer, string schema) {
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

            private readonly ServiceBusEventQueueHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(ServiceBusEventQueueHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IEventProperties properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly ServiceBusEventQueueHarness _outer;
        }

        private readonly IContainer _container;
        private readonly string _queue;
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