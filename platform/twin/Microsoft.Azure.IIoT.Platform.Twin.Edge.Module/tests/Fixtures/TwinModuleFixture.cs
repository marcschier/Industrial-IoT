// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Tests {
    using Microsoft.Azure.IIoT.Platform.Twin.Clients;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Twin.Api;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Mock;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using Opc.Ua;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage.Default;

    /// <summary>
    /// Harness for opc twin module
    /// </summary>
    public class TwinModuleFixture : IInjector, ITwinModuleConfig, IDisposable {

        /// <summary>
        /// Hub
        /// </summary>
        public string Hub { get; }

        /// <summary>
        /// Device id
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// Module id
        /// </summary>
        public string ModuleId { get; }

        /// <summary>
        /// Gateway
        /// </summary>
        public string Gateway { get; }

        /// <summary>
        /// ServerPkiRootPath
        /// </summary>
        public string ServerPkiRootPath { get; }

        /// <summary>
        /// ClientPkiRootPath
        /// </summary>
        public string ClientPkiRootPath { get; }

        /// <summary>
        /// Hub container
        /// </summary>
        public IContainer HubContainer { get; }

        /// <summary>
        /// Create fixture
        /// </summary>
        public TwinModuleFixture() {

            DeviceId = Utils.GetHostName();
            ModuleId = Guid.NewGuid().ToString();

            ServerPkiRootPath = Path.Combine(Directory.GetCurrentDirectory(), "pki",
               Guid.NewGuid().ToByteArray().ToBase16String());
            ClientPkiRootPath = Path.Combine(Directory.GetCurrentDirectory(), "pki",
               Guid.NewGuid().ToByteArray().ToBase16String());

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "PkiRootPath", ClientPkiRootPath }
                })
                .Build();
            HubContainer = CreateHubContainer();
            _hub = HubContainer.Resolve<IDeviceTwinServices>();
            Hub = _hub.HostName;

            // Create gateway identitity
            var gw = _hub.CreateOrUpdateAsync(new DeviceTwinModel {
                Id = DeviceId,
                Tags = new Dictionary<string, VariantValue> {
                    { TwinProperty.Type, IdentityType.Gateway }
                }
            }).Result;

            // Create module identitity
            var twin = _hub.CreateOrUpdateAsync(new DeviceTwinModel {
                Id = DeviceId,
                ModuleId = ModuleId
            }).Result;
            _etag = twin.Etag;

            // Get device registration and create module host with controller
            _device = _hub.GetRegistrationAsync(twin.Id, twin.ModuleId).Result;
            _running = false;
            _module = new ModuleProcess(_config, this);
            var tcs = new TaskCompletionSource<bool>();
            _module.OnRunning += (_, e) => tcs.TrySetResult(e);
            _process = Task.Run(() => _module.RunAsync());

            // Wait
            _running = tcs.Task.Result;
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_running) {
                _module.Exit(1);
                var result = _process.Result;
                Assert.Equal(1, result);
                _running = false;
            }
            if (Directory.Exists(ServerPkiRootPath)) {
                Try.Op(() => Directory.Delete(ServerPkiRootPath, true));
            }
            HubContainer.Dispose();
        }

        /// <inheritdoc/>
        public void Inject(ContainerBuilder builder) {

            // Register mock iot hub
            builder.RegisterInstance(_hub)
                .AsImplementedInterfaces().ExternallyOwned();

            // Only configure if not yet running - otherwise we use twin host config.
            if (!_running) {
                builder.RegisterInstance(new TestModuleConfig(_device))
                    .AsImplementedInterfaces();
            }

            // Add mock sdk
            builder.RegisterModule<IoTHubMockModule>();

            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();


            // Override client config
            builder.RegisterInstance(_config).AsImplementedInterfaces();
            builder.RegisterType<TestClientServicesConfig>()
                .AsImplementedInterfaces();
        }

        /// <summary>
        /// Twin Module supervisor test harness
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public async Task RunTestAsync(Func<string, string, string, IContainer, Task> test) {
            AssertRunning();
            try {
                await test(Hub, DeviceId, ModuleId, HubContainer);
            }
            finally {
                _module.Exit(1);
                var result = await _process;
                Assert.Equal(1, result);
                _running = false;
            }
            AssertStopped();
        }

        /// <summary>
        /// Twin module twin test harness
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public async Task RunTestAsync(EndpointModel ep,
            Func<EndpointInfoModel, IContainer, Task> test) {
            var endpoint = new EndpointInfoModel {
                Endpoint = ep,
                SupervisorId = HubResource.Format(Hub, DeviceId, ModuleId)
            };
            AssertRunning();
            try {
                endpoint = RegisterAndActivateTwinId(endpoint);
                await test(endpoint, HubContainer);
                DeactivateTwinId(endpoint);
            }
            finally {
                _module.Exit(1);
                var result = await _process;
                Assert.Equal(1, result);
                _running = false;
            }
            AssertStopped();
        }

        private void AssertStopped() {
            Assert.False(_running);
            var twin = _hub.GetAsync(DeviceId, ModuleId).Result;
            // TODO : Fix cleanup!!!
            // TODO :Assert.NotEqual("testType", twin.Properties.Reported[TwinProperty.kType]);
            // TODO :Assert.Equal("Disconnected", twin.ConnectionState);
            Assert.NotEqual(_etag, twin.Etag);
        }

        /// <summary>
        /// Assert module running
        /// </summary>
        public void AssertRunning() {
            Assert.True(_running);
            var twin = _hub.GetAsync(DeviceId, ModuleId).Result;
            // Assert
            Assert.Equal("Connected", twin.ConnectionState);
            Assert.Equal(IdentityType.Supervisor, twin.Properties.Reported[TwinProperty.Type]);
        }

        /// <summary>
        /// Activate
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public EndpointInfoModel RegisterAndActivateTwinId(EndpointInfoModel endpoint) {
            endpoint.ApplicationId = "uas" + Guid.NewGuid().ToString();

            var registry = HubContainer.Resolve<IEndpointRegistry>();
            var endpoints = registry.ListAllEndpointsAsync().Result;
            var ep1 = endpoints.FirstOrDefault();

            if (ep1.ActivationState == EntityActivationState.Deactivated) {
                // Activate
                registry.ActivateEndpointAsync(ep1.Id).Wait();
            }
            return ep1;
        }

        /// <summary>
        /// Deactivate
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public void DeactivateTwinId(EndpointInfoModel endpoint) {
            var registry = HubContainer.Resolve<IEndpointRegistry>();
            // Deactivate
            registry.DeactivateEndpointAsync(endpoint.Id).Wait();
        }

        /// <inheritdoc/>
        public class TestModuleConfig : IIoTEdgeClientConfig {

            /// <inheritdoc/>
            public TestModuleConfig(DeviceModel device) {
                _device = device;
            }

            /// <inheritdoc/>
            public string EdgeHubConnectionString =>
                ConnectionString.CreateModuleConnectionString("test.test.org",
                    _device.Id, _device.ModuleId, _device.Authentication.PrimaryKey)
                .ToString();

            /// <inheritdoc/>
            public bool BypassCertVerification => true;

            /// <inheritdoc/>
            public string Product => "Test";

            /// <inheritdoc/>
            public TransportOption Transport => TransportOption.Any;

            private readonly DeviceModel _device;
        }

        /// <inheritdoc/>
        public class TestIoTHubConfig : IIoTHubConfig {

            /// <inheritdoc/>
            public string IoTHubConnString =>
                ConnectionString.CreateServiceConnectionString(
                    "test.test.org", "iothubowner", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))).ToString();
        }

        /// <summary>
        /// Create hub container
        /// </summary>
        /// <returns></returns>
        private IContainer CreateHubContainer() {
            var builder = new ContainerBuilder();

            builder.RegisterModule<NewtonSoftJsonModule>();
            builder.RegisterInstance(this).AsImplementedInterfaces();
            builder.RegisterInstance(_config).AsImplementedInterfaces();
            builder.AddDebugDiagnostics();
            builder.RegisterModule<IoTHubMockService>();
            builder.RegisterType<TestIoTHubConfig>()
                .AsImplementedInterfaces();

            // Twin and history clients
            builder.RegisterType<TwinModuleControlClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinModuleSupervisorClient>()
                .AsImplementedInterfaces();

            builder.RegisterType<HistoryRawSupervisorAdapter>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinSupervisorAdapter>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinModuleClient>()
                .AsImplementedInterfaces();

            // Adapts to expanded hda
            builder.RegisterType<HistoricAccessAdapter<string>>()
                .AsImplementedInterfaces();
            builder.RegisterType<HistoricAccessAdapter<EndpointInfoModel>>()
                .AsImplementedInterfaces();
            builder.RegisterType<HistoricAccessAdapter<EndpointApiModel>>()
                .AsImplementedInterfaces();

            // Supervisor clients
            builder.RegisterType<TwinModuleActivationClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinModuleCertificateClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinModuleDiagnosticsClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscovererModuleClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<VariantEncoderFactory>()
                .AsImplementedInterfaces();

            // Add services
            builder.RegisterModule<RegistryServices>();
            builder.RegisterType<ApplicationDatabase>()
                .AsImplementedInterfaces();
            builder.RegisterModule<EventBrokerStubs>();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            return builder.Build();
        }

        private readonly IDeviceTwinServices _hub;
        private readonly string _etag;
        private readonly DeviceModel _device;
        private bool _running;
        private readonly ModuleProcess _module;
        private readonly IConfiguration _config;
        private readonly Task<int> _process;
        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
