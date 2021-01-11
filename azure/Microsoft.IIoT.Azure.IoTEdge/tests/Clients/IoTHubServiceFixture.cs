// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.EventHub.Processor.Runtime;
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using Xunit;
    using System;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    [CollectionDefinition(Name)]
    public class IoTHubServiceCollection : ICollectionFixture<IoTHubServiceFixture> {

        public const string Name = "Server";
    }

    public sealed class IoTHubServiceFixture : IDisposable {

        public static bool Up => _container != null;

        /// <summary>
        /// Register consumer
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="consumer"></param>
        public static void Register(string resource, IEventConsumer consumer) {
            if (!Up) {
                throw new ResourceInvalidStateException("No service");
            }
            var consumers = _container.Resolve<EventConsumerPerTarget>();
            consumers.Register(resource, consumer);
        }

        /// <summary>
        /// Unregister consumer
        /// </summary>
        /// <param name="resource"></param>
        public static void Unregister(string resource) {
            if (!Up) {
                throw new ResourceInvalidStateException("No service");
            }
            var consumers = _container.Resolve<EventConsumerPerTarget>();
            consumers.Unregister(resource);
        }

        /// <summary>
        /// Create fixture
        /// </summary>
        public IoTHubServiceFixture() {
            if (Interlocked.Increment(ref _refcount) == 1) {
                try {
                    // Read connections string from keyvault
                    var config = new ConfigurationBuilder()
                        .AddFromDotEnvFile()
                        .AddFromKeyVault()
                        .Build();

                    var builder = new ContainerBuilder();
                    builder.AddConfiguration(config);
                    builder.RegisterModule<IoTHubSupportModule>();

                    builder.RegisterModule<EventHubProcessorModule>();
                    builder.RegisterType<IoTHubConsumerConfig>()
                        .AsImplementedInterfaces(); // Point to iot hub

                    // Singleton here to allow access from tests
                    builder.RegisterType<EventConsumerPerTarget>()
                        .AsSelf()
                        .AsImplementedInterfaces().SingleInstance();

                    // Start event processor
                    builder.RegisterType<HostAutoStart>()
                        .AutoActivate()
                        .AsImplementedInterfaces().SingleInstance();

                    builder.AddDiagnostics();
                    _container = builder.Build();
                }
                catch {
                    Interlocked.Decrement(ref _refcount);
                    _container = null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (Interlocked.Decrement(ref _refcount) == 0) {
                _container?.Dispose();
                _container = null;
            }
        }

        private static IContainer _container;
        private static int _refcount;
    }


    /// <summary>
    /// Default device event handler implementation
    /// </summary>
    public sealed class EventConsumerPerTarget : IEventConsumer {

        public EventConsumerPerTarget(IOptions<IoTHubServiceOptions> options) {
            _hostName = ConnectionString.Parse(options.Value.ConnectionString).HostName;
        }

        /// <summary>
        /// Register consumer factory
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="consumer"></param>
        public void Register(string resource, IEventConsumer consumer) {
            _handlers.AddOrUpdate(resource, consumer);
        }

        /// <summary>
        /// Unregister consumer factory
        /// </summary>
        /// <param name="resource"></param>
        public void Unregister(string resource) {
            _handlers.TryRemove(resource, out _);
        }

        /// <inheritdoc/>
        public async Task HandleAsync(byte[] eventData,
            IEventProperties properties, Func<Task> checkpoint) {
            if (!properties.TryGetValue(SystemProperties.ConnectionDeviceId, out var deviceId) &&
                !properties.TryGetValue(SystemProperties.DeviceId, out deviceId)) {
                // Not from a device
                return;
            }

            if (!properties.TryGetValue(SystemProperties.ConnectionModuleId, out var moduleId) &&
                !properties.TryGetValue(SystemProperties.ModuleId, out moduleId)) {
                // Not from a module
                moduleId = null;
            }

            var target = HubResource.Format(_hostName, deviceId, moduleId);
            if (_handlers.TryGetValue(target, out var handler)) {
                await handler.HandleAsync(eventData, properties, checkpoint).ConfigureAwait(false);
                _used.AddOrUpdate(target, true);
            }
        }

        /// <inheritdoc/>
        public async Task OnBatchCompleteAsync() {
            foreach (var target in _used.Keys.ToList()) {
                if (_handlers.TryGetValue(target, out var handler)) {
                    await Try.Async(handler.OnBatchCompleteAsync).ConfigureAwait(false);
                    _used.TryRemove(target, out _);
                }
            }
        }

        private readonly ConcurrentDictionary<string, IEventConsumer> _handlers = new();
        private readonly ConcurrentDictionary<string, bool> _used = new();
        private readonly string _hostName;
    }
}