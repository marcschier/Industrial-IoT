// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public class ServiceBusEventQueueFixture : IDisposable {

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        public ServiceBusEventQueueHarness GetHarness(string queue) {
            return new ServiceBusEventQueueHarness(queue);
        }

        /// <inheritdoc/>
        public void Dispose() {
        }
    }

    public class ServiceBusQueueConfig : IServiceBusProcessorConfig {
        public ServiceBusQueueConfig(string queue) {
            Queue = queue;
        }
        public string Queue { get; }
    }

    public class ServiceBusEventQueueHarness : IDisposable {

        public event TelemetryEventHandler OnEvent;
        public event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        public ServiceBusEventQueueHarness(string queue) {
            try {
                var builder = new ContainerBuilder();

                // Read connections string from keyvault
                var config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();
                builder.RegisterInstance(config)
                    .AsImplementedInterfaces();
                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();

                // Set queue name
                builder.RegisterInstance(new ServiceBusQueueConfig(queue))
                    .AsImplementedInterfaces();

                builder.RegisterModule<ServiceBusEventQueueModule>();
                builder.RegisterModule<ServiceBusEventProcessorModule>();
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

                builder.AddDebugDiagnostics();
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
        public IEventQueueClient GetEventQueueClient() {
            return Try.Op(() => _container?.Resolve<IEventQueueClient>());
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
            var config = _container.Resolve<IServiceBusConfig>();
            var managementClient = new ManagementClient(config.ServiceBusConnString);
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
                byte[] payload, IDictionary<string, string> properties,
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
                IDictionary<string, string> properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly ServiceBusEventQueueHarness _outer;
        }

        private readonly IContainer _container;
        private readonly string _queue;
    }

    public class TelemetryEventArgs : EventArgs {

        public TelemetryEventArgs(string schema, string source,
            byte[] data, IDictionary<string, string> properties) {
            HandlerSchema = schema;
            Source = source;
            try {
                DeviceId = HubResource.Parse(source, out var hub, out var moduleId);
                Hub = hub;
                ModuleId = moduleId;
            }
            catch {

            }
            Data = data;
            Target = properties.TryGetValue(EventProperties.Target, out var v) ? v : null;
            Properties = properties
                .Where(k => k.Key != EventProperties.Target)
                .Where(k => !k.Key.StartsWith("x-"))
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public string Target { get; }
        public string Source { get; }
        public string HandlerSchema { get; }
        public string Hub { get; }
        public string DeviceId { get; }
        public string ModuleId { get; }
        public byte[] Data { get; }
        public IDictionary<string, string> Properties { get; }
    }

    public delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}