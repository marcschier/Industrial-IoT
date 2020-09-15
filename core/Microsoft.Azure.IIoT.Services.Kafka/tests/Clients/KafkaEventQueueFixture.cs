// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Clients {
    using Microsoft.Azure.IIoT.Services.Kafka.Runtime;
    using Microsoft.Azure.IIoT.Services.Kafka.Server;
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hosting;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public class KafkaEventQueueFixture : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public KafkaEventQueueFixture() {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterType<KafkaConsumerConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<KafkaCluster>()
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
        /// <param name="topic"></param>
        /// <returns></returns>
        public KafkaEventQueueHarness GetHarness(string topic) {
            return new KafkaEventQueueHarness(topic);
        }

        /// <summary>
        /// Clean up query container
        /// </summary>
        public void Dispose() {
            _container?.Dispose();
        }

        private readonly IContainer _container;
    }

    public class Pid : IProcessIdentity {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ServiceId { get; } = Guid.NewGuid().ToString();
        public string Name { get; } = "test";
        public string Description { get; } = "the test";
    }

    /// <summary>
    /// Event processor configuration - wraps a configuration root
    /// </summary>
    public class KafkaConsumerConfig : KafkaServerConfig, IKafkaConsumerConfig {
        /// <inheritdoc/>
        public string ConsumerGroup => "$default";
        /// <inheritdoc/>
        public string ConsumerTopic { get; set; }
        /// <inheritdoc/>
        public int ReceiveBatchSize => 10;
        /// <inheritdoc/>
        public TimeSpan ReceiveTimeout => TimeSpan.FromSeconds(5);
        /// <inheritdoc/>
        public bool InitialReadFromEnd => false;
        /// <inheritdoc/>
        public TimeSpan? SkipEventsOlderThan => null;
        /// <inheritdoc/>
        public TimeSpan? CheckpointInterval => TimeSpan.FromMinutes(1);
    }

    public class KafkaEventQueueHarness : IDisposable {

        public event TelemetryEventHandler OnEvent;
        public event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        public KafkaEventQueueHarness(string topic) {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<KafkaProducerModule>();
                builder.RegisterModule<KafkaConsumerModule>();
                builder.RegisterType<KafkaServerConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterInstance(new KafkaConsumerConfig { ConsumerTopic = topic })
                    .AsImplementedInterfaces().SingleInstance();
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

            public TestHandler(KafkaEventQueueHarness outer, string schema) {
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

            private readonly KafkaEventQueueHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(KafkaEventQueueHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IDictionary<string, string> properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly KafkaEventQueueHarness _outer;
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
                .Where(k => k.Key != "x-topic")
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