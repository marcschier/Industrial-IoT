// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting.Services {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Mock;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ModuleHostHarness {

        /// <summary>
        /// Module test harness
        /// </summary>
        /// <param name="controllers"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public async Task RunTestAsync(
            IEnumerable<object> controllers, Func<string, string, IContainer, Task> test) {

            var deviceId = "TestDevice";
            var moduleId = "TestModule";

            using (var hubContainer = CreateHubContainer()) {
                var services = hubContainer.Resolve<IDeviceTwinServices>();

                // Create module
                var twin = await services.CreateOrUpdateAsync(new DeviceTwinModel {
                    Id = "TestDevice",
                    ModuleId = "TestModule"
                }).ConfigureAwait(false);
                var etag = twin.Etag;
                var device = await services.GetRegistrationAsync(twin.Id, twin.ModuleId).ConfigureAwait(false);

                // Create module host with controller
                using (var moduleContainer = CreateModuleContainer(services, device,
                    controllers)) {
                    var edge = moduleContainer.Resolve<IModuleHost>();

                    // Act
                    await edge.StartAsync("testType", "1.2.3").ConfigureAwait(false);
                    twin = await services.GetAsync(deviceId, moduleId).ConfigureAwait(false);

                    // Assert
                    Assert.NotEqual(etag, twin.Etag);
                    Assert.Equal("Connected", twin.ConnectionState);
                    Assert.Equal("testType", twin.Properties.Reported[TwinProperty.Type]);
                    etag = twin.Etag;

                    await test(deviceId, moduleId, hubContainer).ConfigureAwait(false);

                    twin = await services.GetAsync(deviceId, moduleId).ConfigureAwait(false);
                    Assert.True(twin.Properties.Reported[TwinProperty.Type] == "testType");
                    etag = twin.Etag;

                    // Act
                    await edge.StopAsync().ConfigureAwait(false);
                    twin = await services.GetAsync(deviceId, moduleId).ConfigureAwait(false);

                    // TODO : Fix cleanup!!!

                    // TODO :Assert.True("testType" != twin.Properties.Reported[TwinProperty.kType]);

                    // TODO :Should we allow closing and re-creating like before?
                    // TODO :Assert.Equal("Disconnected", twin.ConnectionState);
                }
            }
        }

        public class TestIoTHubConfig : IIoTHubConfig {
            public string IoTHubConnString =>
                ConnectionString.CreateServiceConnectionString(
                    "test.test.org", "iothubowner", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))).ToString();
        }

        public class TestModuleConfig : IIoTEdgeClientConfig, IDiagnosticsConfig {

            public TestModuleConfig(DeviceModel device) {
                _device = device;
            }
            public string EdgeHubConnectionString =>
                ConnectionString.CreateModuleConnectionString("test.test.org",
                    _device.Id, _device.ModuleId, _device.Authentication.PrimaryKey)
                .ToString();

            public bool BypassCertVerification => true;

            public string Product => "test";

            public TransportOption Transport => TransportOption.Any;

            public DiagnosticsLevel DiagnosticsLevel => DiagnosticsLevel.Disabled;

            public TimeSpan? MetricsCollectionInterval => null;

            private readonly DeviceModel _device;
        }

        /// <summary>
        /// Create hub container
        /// </summary>
        /// <returns></returns>
        private IContainer CreateHubContainer() {
            var builder = new ContainerBuilder();
            builder.AddDebugDiagnostics();
            builder.RegisterModule<IoTHubMockService>();
            builder.RegisterType<TestIoTHubConfig>()
                .AsImplementedInterfaces();
            return builder.Build();
        }

        /// <summary>
        /// Create module container
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="device"></param>
        /// <param name="controllers"></param>
        /// <returns></returns>
        private IContainer CreateModuleContainer(IDeviceTwinServices hub, DeviceModel device,
            IEnumerable<object> controllers) {
            var builder = new ContainerBuilder();
            builder.AddDebugDiagnostics();
            builder.RegisterInstance(hub)
                .AsImplementedInterfaces().ExternallyOwned();
            builder.RegisterInstance(new TestModuleConfig(device))
                .AsImplementedInterfaces();
            builder.RegisterModule<IoTHubMockModule>();
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            foreach (var controller in controllers) {
                builder.RegisterInstance(controller)
                    .AsImplementedInterfaces();
            }
            return builder.Build();
        }
    }
}
