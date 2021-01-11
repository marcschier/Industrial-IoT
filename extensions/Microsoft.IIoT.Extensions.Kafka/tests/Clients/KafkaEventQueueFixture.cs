// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka.Clients {
    using Microsoft.IIoT.Extensions.Kafka.Runtime;
    using Microsoft.IIoT.Extensions.Messaging.Handlers;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Utils;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public sealed class KafkaEventQueueFixture {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        internal KafkaEventQueueHarness GetHarness(string topic) {
            return new KafkaEventQueueHarness(topic, KafkaServerFixture.Up && !Skip);
        }
    }

    public class Pid : IProcessIdentity {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ServiceId { get; } = Guid.NewGuid().ToString();
        public string Name { get; } = "test";
        public string Description { get; } = "the test";
    }

    internal sealed class KafkaEventQueueHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        internal KafkaEventQueueHarness(string topic, bool serverUp) {
            if (!serverUp) {
                _container = null;
                return;
            }
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<KafkaProducerModule>();
                builder.RegisterModule<KafkaConsumerModule>();
                builder.RegisterType<KafkaServerConfig>()
                    .AsImplementedInterfaces().SingleInstance();

                builder.Configure<KafkaConsumerOptions>(options => {
                    options.CheckpointInterval = TimeSpan.FromMinutes(1);
                    options.ReceiveTimeout = TimeSpan.FromMinutes(5);
                    options.InitialReadFromEnd = false;
                    options.ReceiveBatchSize = 10;
                    options.ConsumerGroup = "$default";
                    options.ConsumerTopic = topic;
                });

                builder.RegisterType<Pid>()
                    .AsImplementedInterfaces();

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

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _container?.Dispose();
        }

        internal class TestHandler : ITelemetryHandler {

            public TestHandler(KafkaEventQueueHarness outer, string schema) {
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

            private readonly KafkaEventQueueHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(KafkaEventQueueHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IEventProperties properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly KafkaEventQueueHarness _outer;
        }

        private readonly IContainer _container;
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
        public IReadOnlyCollection<byte> Data { get; }
        public IEventProperties Properties { get; }
    }

    internal delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}