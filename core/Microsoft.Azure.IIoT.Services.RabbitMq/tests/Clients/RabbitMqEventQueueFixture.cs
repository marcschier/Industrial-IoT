// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Services.RabbitMq.Runtime;
    using Microsoft.Azure.IIoT.Services.RabbitMq.Server;
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.Azure.IIoT.Services.RabbitMq.Services;

    public sealed class RabbitMqEventQueueFixture : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public RabbitMqEventQueueFixture() {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<RabbitMqEventQueueModule>();
                builder.RegisterType<RabbitMqConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<RabbitMqServer>()
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
#pragma warning disable CA1822 // Mark members as static
        internal RabbitMqEventQueueHarness GetHarness(string queue) {
#pragma warning restore CA1822 // Mark members as static
            return new RabbitMqEventQueueHarness(queue);
        }

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _container?.Dispose();
        }

        private readonly IContainer _container;
    }

    public class RabbitMqQueueConfig : IRabbitMqQueueConfig {
        public RabbitMqQueueConfig(string queue) {
            Queue = queue;
        }
        public string Queue { get; }
    }

    internal sealed class RabbitMqEventQueueHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        public RabbitMqEventQueueHarness(string queue) {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<RabbitMqEventQueueModule>();
                builder.RegisterModule<RabbitMqEventProcessorModule>();
                builder.RegisterInstance(new RabbitMqQueueConfig(queue))
                    .AsImplementedInterfaces();
                builder.RegisterType<RabbitMqConfig>()
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

            public TestHandler(RabbitMqEventQueueHarness outer, string schema) {
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

            private readonly RabbitMqEventQueueHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(RabbitMqEventQueueHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IDictionary<string, string> properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly RabbitMqEventQueueHarness _outer;
        }

        private readonly IContainer _container;
    }

    internal class TelemetryEventArgs : EventArgs {

        internal TelemetryEventArgs(string schema, string source,
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
                .Where(k => !k.Key.StartsWith("x-", StringComparison.Ordinal))
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public string Target { get; }
        public string Source { get; }
        public string HandlerSchema { get; }
        public string Hub { get; }
        public string DeviceId { get; }
        public string ModuleId { get; }
        public byte[] Data { get; }
        public IReadOnlyDictionary<string, string> Properties { get; }
    }

    internal delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}