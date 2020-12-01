// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Clients {
    using Microsoft.IIoT.Azure.EventHub.Runtime;
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Messaging.Handlers;
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Utils;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Hosting;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Threading;
    using global::Azure.Identity;
    using global::Azure.ResourceManager.EventHubs;
    using global::Azure.ResourceManager.EventHubs.Models;

    public sealed class EventHubQueueFixture : IDisposable {

        public bool Skip => _config == null;

        public EventHubQueueFixture() {
            try {
                // Read connections string from keyvault
                _config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();
            }
            catch {
                _config = null;
            }
        }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal EventHubQueueHarness GetHarness(string path) {
            return GetHarnessAsync(path).Result;
        }


        /// <inheritdoc/>
        public void Dispose() {
            _limit.Dispose();
        }


        private async Task<EventHubQueueHarness> GetHarnessAsync(string eventHub) {
            if (Skip) {
                return new EventHubQueueHarness(eventHub, null);
            }
            await _limit.WaitAsync().ConfigureAwait(false); // Acquire
            try {
                var eventHubClient = CreateEventHubClient(out var resourceGroup, out var namespaceName);
                await eventHubClient.EventHubs.CreateOrUpdateAsync(resourceGroup, namespaceName,
                    eventHub, new Eventhub {
                        PartitionCount = 4,
                        MessageRetentionInDays = 1,
                        Status = EntityStatus.Active
                    }).ConfigureAwait(false);

                var result = await eventHubClient.EventHubs.CreateOrUpdateAuthorizationRuleAsync(
                    resourceGroup, namespaceName, eventHub, eventHub + "-key",
                    new AuthorizationRule {
                        Rights = new List<AccessRights> {
                            AccessRights.Listen, AccessRights.Send
                        }
                    }).ConfigureAwait(false);
                var keys = await eventHubClient.EventHubs.ListKeysAsync(resourceGroup,
                    namespaceName, eventHub, eventHub + "-key").ConfigureAwait(false);
                return new EventHubQueueHarness(keys.Value.PrimaryConnectionString, async () => {
                    try {
                        await eventHubClient.EventHubs.DeleteAsync(resourceGroup, namespaceName,
                            eventHub).ConfigureAwait(false);
                    }
                    finally {
                        _limit.Release();
                    }
                });
            }
            catch {
                _limit.Release(); // release
                return new EventHubQueueHarness(eventHub, null);
            }
        }

        /// <summary>
        /// Create management client
        /// </summary>
        /// <param name="resourceGroup"></param>
        /// <param name="namespaceName"></param>
        /// <returns></returns>
        private EventHubsManagementClient CreateEventHubClient(out string resourceGroup,
            out string namespaceName) {
            var subscriptionId = _config.GetValue<string>("PCS_SUBSCRIPTION_ID");
            resourceGroup = _config.GetValue<string>("PCS_RESOURCE_GROUP");
            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(resourceGroup)) {
                throw new ResourceInvalidStateException("Subscription or group not configured");
            }

            var eh = new EventHubClientConfig(_config).ToOptions();
            namespaceName = ConnectionString.Parse(eh.Value.EventHubConnString)?.Endpoint;
            namespaceName = namespaceName?.Replace("sb://", "",
                StringComparison.InvariantCulture).Split('.')[0];

            var tenantId = _config.GetValue<string>("PCS_AUTH_TENANT");
            var credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions {
                SharedTokenCacheTenantId = tenantId,
                VisualStudioTenantId = tenantId,
                VisualStudioCodeTenantId = tenantId,
                InteractiveBrowserTenantId = tenantId
            });
            return new EventHubsManagementClient(subscriptionId, credentials);
        }

        private readonly IConfiguration _config;
        private readonly SemaphoreSlim _limit = new SemaphoreSlim(7); // max 10 per namespace
    }

    public class Pid : IProcessIdentity {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ServiceId { get; } = Guid.NewGuid().ToString();
        public string Name { get; } = "test";
        public string Description { get; } = "the test";
    }

    internal sealed class EventHubQueueHarness : IDisposable {

        internal event TelemetryEventHandler OnEvent;
        internal event EventHandler OnComplete;

        /// <summary>
        /// Create fixture
        /// </summary>
        internal EventHubQueueHarness(string connectionString, Action dispose) {
            if (dispose == null || connectionString == null) {
                _dispose = null;
                _container = null;
                return; // Failed to get event hub
            }
            try {
                var builder = new ContainerBuilder();

                // Read connections string from keyvault
                var config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();
                builder.AddConfiguration(config);
                builder.Configure<EventHubConsumerOptions>(options => {
                    options.EventHubConnString = connectionString;
                    options.EventHubPath = null;
                });
                builder.RegisterType<Pid>().SingleInstance()
                    .AsImplementedInterfaces();

                builder.RegisterModule<EventHubClientModule>();
                builder.RegisterModule<EventHubProcessorModule>();

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
                _dispose = dispose;
            }
            catch {
                _container = null;
                _dispose = null;
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
            if (_container == null) {
                return;
            }
            _dispose.Invoke();
            _container.Dispose();
        }

        internal class TestHandler : ITelemetryHandler {

            public TestHandler(EventHubQueueHarness outer, string schema) {
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

            private readonly EventHubQueueHarness _outer;
        }

        internal class UnknownHandler : IUnknownEventProcessor {

            public UnknownHandler(EventHubQueueHarness outer) {
                _outer = outer;
            }

            public Task HandleAsync(byte[] eventData,
                IDictionary<string, string> properties) {
                _outer.OnEvent?.Invoke(this, new TelemetryEventArgs(
                    null, null, eventData, properties));
                return Task.CompletedTask;
            }

            private readonly EventHubQueueHarness _outer;
        }

        private readonly IContainer _container;
        private readonly Action _dispose;
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
        public IReadOnlyCollection<byte> Data { get; }
        public IReadOnlyDictionary<string, string> Properties { get; }
    }

    internal delegate void TelemetryEventHandler(object sender, TelemetryEventArgs args);
}