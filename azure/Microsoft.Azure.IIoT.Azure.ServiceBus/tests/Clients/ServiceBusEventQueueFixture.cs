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
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public class ServiceBusEventQueueFixture : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public ServiceBusEventQueueFixture() {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();

                builder.AddDebugDiagnostics();
                _container = builder.Build();
            }
            catch {
                _container = null;
            }
        }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        public ServiceBusEventQueueHarness GetHarness(string topic) {
            return new ServiceBusEventQueueHarness();
        }

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _container?.Dispose();
        }

        private readonly IContainer _container;
    }

    public class ServiceBusEventQueueHarness : IDisposable {

        public event TelemetryEventHandler OnEvent;
        public event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        public ServiceBusEventQueueHarness() {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<ServiceBusEventQueueModule>();
               // builder.RegisterModule<RabbitMqEventProcessorModule>();
                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();

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

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _container?.Dispose();
        }

        internal class TestHandler : ITelemetryHandler {

            public TestHandler(ServiceBusEventQueueHarness outer, string schema) {
                _outer = outer;
                MessageSchema = schema;
            }

            public string MessageSchema { get; }

            public Task HandleAsync(string deviceId, string moduleId,
                byte[] payload, IDictionary<string, string> properties,
                Func<Task> checkpoint) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    MessageSchema, deviceId, moduleId, payload, properties));
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
                    null, null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly ServiceBusEventQueueHarness _outer;
        }

        private readonly IContainer _container;
    }

    public class TelemetryEventArgs : EventArgs {

        public TelemetryEventArgs(string schema, string deviceId,
            string moduleId, byte[] data, IDictionary<string, string> properties) {
            HandlerSchema = schema;
            DeviceId = deviceId;
            ModuleId = moduleId;
            Data = data;
            Target = properties.TryGetValue(EventProperties.Target, out var v) ? v : null;
            Properties = properties
                .Where(k => k.Key != EventProperties.Target)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public string Target { get; }
        public string HandlerSchema { get; }
        public string DeviceId { get; }
        public string ModuleId { get; }
        public byte[] Data { get; }
        public IDictionary<string, string> Properties { get; }
    }

    public delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}