// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Clients {
    using Microsoft.IIoT.Extensions.RabbitMq.Runtime;
    using Microsoft.IIoT.Messaging.Handlers;
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Utils;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public sealed class RabbitMqEventQueueFixture {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        public RabbitMqEventQueueHarness GetHarness(string queue) {
            if (Skip || !RabbitMqServerFixture.Up) {
                return new RabbitMqEventQueueHarness();
            }
            return new RabbitMqEventQueueHarness(queue);
        }
    }

    public sealed class RabbitMqEventQueueHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        public RabbitMqEventQueueHarness(string queue) {
            try {
                var clientBuilder = new ContainerBuilder();

                clientBuilder.RegisterModule<RabbitMqEventQueueModule>();
                clientBuilder.RegisterType<RabbitMqConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                clientBuilder.AddDiagnostics();
                _client = clientBuilder.Build();

                var consumerBuilder = new ContainerBuilder();

                consumerBuilder.RegisterModule<RabbitMqEventProcessorModule>();
                consumerBuilder.Configure<RabbitMqQueueOptions>(options => options.Queue = queue);
                consumerBuilder.RegisterType<RabbitMqConfig>()
                    .AsImplementedInterfaces().SingleInstance();

                consumerBuilder.RegisterType<DeviceEventHandler>()
                    .AsImplementedInterfaces().InstancePerDependency();
                consumerBuilder.RegisterInstance(new TestHandler(this, "Test1"))
                    .AsImplementedInterfaces();
                consumerBuilder.RegisterInstance(new TestHandler(this, "Test2"))
                    .AsImplementedInterfaces();
                consumerBuilder.RegisterInstance(new TestHandler(this, "Test3"))
                    .AsImplementedInterfaces();
                consumerBuilder.RegisterInstance(new UnknownHandler(this))
                    .AsImplementedInterfaces();
                consumerBuilder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();

                consumerBuilder.AddDiagnostics();
                _consumer = consumerBuilder.Build();
            }
            catch {
                _client = null;
                _consumer = null;
            }
        }

        public RabbitMqEventQueueHarness() {
            _client = null;
            _consumer = null;
        }

        /// <summary>
        /// Get Event Queue client
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventPublisherClient GetEventPublisherClient() {
            return Try.Op(() => _client?.Resolve<IEventPublisherClient>());
        }

        /// <summary>
        /// Get Event client
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventClient GetEventClient() {
            return Try.Op(() => _client?.Resolve<IEventClient>());
        }

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _client?.Dispose();
            _consumer?.Dispose();
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

        private readonly IContainer _client;
        private readonly IContainer _consumer;
    }

    internal class TelemetryEventArgs : EventArgs {

        internal TelemetryEventArgs(string schema, string source,
            byte[] data, IDictionary<string, string> properties) {
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