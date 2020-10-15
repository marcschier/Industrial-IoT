// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Supervisor {
    using Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hub;
    using Autofac;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class SupervisorTests {
#if FALSE
        [Fact]
        public async Task TestListSupervisorsAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var registry = services.Resolve<ISupervisorRegistry>();

                    // Act
                    var supervisors = await registry.ListAllSupervisorsAsync().ConfigureAwait(false);

                    // Assert
                    Assert.Single(supervisors);
                    Assert.True(supervisors.Single().Connected.Value);
                    Assert.True(supervisors.Single().OutOfSync.Value);
                    Assert.Equal(device, HubResource.Parse(supervisors.Single().Id,
                        out var hub, out var moduleId));
                    Assert.Equal(hubName, hub);
                    Assert.Equal(module, moduleId);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestGetSupervisorAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var registry = services.Resolve<ISupervisorRegistry>();

                    // Act
                    var supervisor = await registry.GetSupervisorAsync(
                        HubResource.Format(hubName, device, module)).ConfigureAwait(false);

                    // Assert
                    Assert.True(supervisor.Connected.Value);
                    Assert.True(supervisor.OutOfSync.Value);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestGetSupervisorStatusAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var diagnostics = services.Resolve<ISupervisorDiagnostics>();

                    // Act
                    var status = await diagnostics.GetSupervisorStatusAsync(
                        HubResource.Format(hubName, device, module)).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(status.DeviceId, device);
                    Assert.Equal(status.ModuleId, module);
                    Assert.Empty(status.Entities);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestActivateEndpointAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var supervisorId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IEndpointRegistry>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new EndpointInfoModel {
                        Endpoint = new EndpointModel {
                            Url = "opc.tcp://test"
                        },
                        SupervisorId = supervisorId,
                        ApplicationId = "ua326029342304923"
                    }.ToDocumentModel(_serializer).ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IEndpointRegistry>();
                    var endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep1 = endpoints.FirstOrDefault();
                    Assert.NotNull(ep1);

                    // Act
                    await activation.ActivateEndpointAsync(ep1.Id).ConfigureAwait(false);
                    endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep2 = endpoints.FirstOrDefault();
                    var diagnostics = services.Resolve<ISupervisorDiagnostics>();
                    var status = await diagnostics.GetSupervisorStatusAsync(supervisorId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Single(status.Entities);
                    Assert.Equal(ep1.Id, status.Entities.Single().Id);
                    Assert.Equal(EntityActivationState.Activated, status.Entities.Single().ActivationState);
                    Assert.Equal(EntityActivationState.Activated, ep2.ActivationState);
                    Assert.True(
                        ep2.EndpointState == EndpointConnectivityState.Connecting ||
                        ep2.EndpointState == EndpointConnectivityState.NotReachable);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestActivateDeactivateEndpointAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var supervisorId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IEndpointRegistry>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new EndpointInfoModel {
                        Endpoint = new EndpointModel {
                            Url = "opc.tcp://test"
                        },
                        SupervisorId = supervisorId,
                        ApplicationId = "ua326029342304923"
                    }.ToDocumentModel(_serializer).ToDeviceTwin(_serializer);

                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IEndpointRegistry>();
                    var endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep1 = endpoints.FirstOrDefault();
                    Assert.NotNull(ep1);

                    // Act
                    await activation.ActivateEndpointAsync(ep1.Id).ConfigureAwait(false);
                    endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep2 = endpoints.FirstOrDefault();
                    await activation.DeactivateEndpointAsync(ep2.Id).ConfigureAwait(false);
                    var diagnostics = services.Resolve<ISupervisorDiagnostics>();
                    endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep3 = endpoints.FirstOrDefault();
                    var status = await diagnostics.GetSupervisorStatusAsync(supervisorId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Empty(status.Entities);
                    Assert.Equal(EntityActivationState.Deactivated, ep3.ActivationState);
                    Assert.Equal(EndpointConnectivityState.Disconnected, ep3.EndpointState);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestActivateDeactivateEndpoint20TimesAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var supervisorId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IEndpointRegistry>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new EndpointInfoModel {
                        Endpoint = new EndpointModel {
                            Url = "opc.tcp://test"
                        },
                        SupervisorId = supervisorId,
                        ApplicationId = "ua326029342304923"
                    }.ToDocumentModel(_serializer).ToDeviceTwin(_serializer);

                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IEndpointRegistry>();
                    var endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep1 = endpoints.FirstOrDefault();
                    Assert.NotNull(ep1);

                    for (var i = 0; i < 20; i++) {
                        // Act
                        await activation.ActivateEndpointAsync(ep1.Id).ConfigureAwait(false);
                        await activation.DeactivateEndpointAsync(ep1.Id).ConfigureAwait(false);
                    }

                    var diagnostics = services.Resolve<ISupervisorDiagnostics>();
                    endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var ep3 = endpoints.FirstOrDefault();
                    var status = await diagnostics.GetSupervisorStatusAsync(supervisorId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Empty(status.Entities);
                    Assert.Equal(EntityActivationState.Deactivated, ep3.ActivationState);
                    Assert.Equal(EndpointConnectivityState.Disconnected, ep3.EndpointState);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestActivateDeactivate20Endpoints5TimesMultiThreadedAsync() {
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device,module, services) => {

                    // Setup
                    var supervisorId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IEndpointRegistry>();
                    var hub = services.Resolve<IDeviceTwinServices>();

                    for (var i = 0; i < 20; i++) {
                        var twin = new EndpointInfoModel {
                            Endpoint = new EndpointModel {
                                Url = "opc.tcp://test"
                            },
                            SupervisorId = supervisorId,
                            ApplicationId = "uas" + i
                        }.ToDocumentModel(_serializer).ToDeviceTwin(_serializer);
                        await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    }

                    var registry = services.Resolve<IEndpointRegistry>();
                    var endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);

                    for (var i = 0; i < 5; i++) {
                        await Task.WhenAll(endpoints.Select(ep => activation.ActivateEndpointAsync(ep.Id))).ConfigureAwait(false);
                        await Task.WhenAll(endpoints.Select(ep => activation.DeactivateEndpointAsync(ep.Id))).ConfigureAwait(false);
                    }

                    var diagnostics = services.Resolve<ISupervisorDiagnostics>();
                    endpoints = await registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    var status = await diagnostics.GetSupervisorStatusAsync(supervisorId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Empty(status.Entities);
                    Assert.True(endpoints.All(ep => ep.ActivationState == EntityActivationState.Deactivated));
                    Assert.True(endpoints.All(ep => ep.EndpointState == EndpointConnectivityState.Disconnected));
                }).ConfigureAwait(false);
            }
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
#endif
    }
}
