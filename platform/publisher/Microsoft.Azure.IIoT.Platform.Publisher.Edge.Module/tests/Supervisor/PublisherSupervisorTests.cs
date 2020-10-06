// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Supervisor {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hub;
    using Autofac;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class PublisherSupervisorTests {
#if FALSE
        [Fact]
        public async Task TestListPublishersAsync() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var registry = services.Resolve<IPublisherRegistry>();

                    // Act
                    var supervisors = await registry.ListAllPublishersAsync().ConfigureAwait(false);

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
        public async Task TestGetPublisherAsync() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var registry = services.Resolve<IPublisherRegistry>();

                    // Act
                    var supervisor = await registry.GetPublisherAsync(
                        HubResource.Format(hubName, device, module)).ConfigureAwait(false);

                    // Assert
                    Assert.True(supervisor.Connected.Value);
                    Assert.True(supervisor.OutOfSync.Value);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestGetPublisherStatusAsync() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var diagnostics = services.Resolve<IPublisherDiagnostics>();

                    // Act
                    var status = await diagnostics.GetPublisherStatusAsync(
                        HubResource.Format(hubName, device, module)).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(status.DeviceId, device);
                    Assert.Equal(status.ModuleId, module);
                    Assert.Empty(status.Entities);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestWriterGroupPlacementAsync() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var publisherId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IWriterGroupOrchestration>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua326029342304923",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IWriterGroupStatus>();
                    var activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    Assert.Empty(activations); // Nothing yet activated

                    // Act
                    await activation.SynchronizeWriterGroupPlacementsAsync().ConfigureAwait(false);
                    activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    var wg2 = activations.FirstOrDefault();
                    var diagnostics = services.Resolve<IPublisherDiagnostics>();
                    var status = await diagnostics.GetPublisherStatusAsync(publisherId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Single(status.Entities);
                    Assert.Equal(wg2.Id, status.Entities.Single().Id);
                    Assert.Equal(EntityActivationState.Activated, status.Entities.Single().ActivationState);
                    Assert.Equal(EntityActivationState.Activated, wg2.ActivationState);
                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestWriterGroupPlacement2Async() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var publisherId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IWriterGroupOrchestration>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049231",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049232",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049233",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IWriterGroupStatus>();
                    var activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    Assert.Empty(activations); // Nothing yet activated

                    // Act
                    await activation.SynchronizeWriterGroupPlacementsAsync().ConfigureAwait(false);
                    activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    var diagnostics = services.Resolve<IPublisherDiagnostics>();
                    var status = await diagnostics.GetPublisherStatusAsync(publisherId).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Equal(3, status.Entities.Count);
                    Assert.Equal(3, activations.Count);
                    Assert.All(status.Entities, e => {
                        Assert.StartsWith("ua26029342304923", e.Id);
                        Assert.Equal(EntityActivationState.Activated, e.ActivationState);
                    });
                    Assert.All(activations, e => {
                        Assert.StartsWith("ua26029342304923", e.Id);
                        Assert.Equal(EntityActivationState.Activated, e.ActivationState);
                    });
                }).ConfigureAwait(false);
            }
        }


        [Fact]
        public async Task TestWriterGroupPlacementWithWrongSiteAsync() {
            using (var harness = new PublisherModuleFixture()) {
                await harness.RunTestAsync(async (hubName, device, module, services) => {

                    // Setup
                    var publisherId = HubResource.Format(hubName, device, module);
                    var activation = services.Resolve<IWriterGroupOrchestration>();
                    var hub = services.Resolve<IDeviceTwinServices>();
                    var twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049231",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049232",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049233",
                        SiteId = device
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    twin = new WriterGroupInfoModel {
                        WriterGroupId = "ua260293423049234",
                        SiteId = "wrong"
                    }.ToWriterGroupRegistration().ToDeviceTwin(_serializer);
                    await hub.CreateOrUpdateAsync(twin).ConfigureAwait(false);
                    var registry = services.Resolve<IWriterGroupStatus>();
                    var activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    Assert.Empty(activations); // Nothing yet activated

                    // Act
                    await activation.SynchronizeWriterGroupPlacementsAsync().ConfigureAwait(false);
                    activations = await registry.ListAllWriterGroupActivationsAsync().ConfigureAwait(false);
                    var diagnostics = services.Resolve<IPublisherDiagnostics>();
                    var status = await diagnostics.GetPublisherStatusAsync(publisherId).ConfigureAwait(false);
                    var includingNotConnected = await registry.ListAllWriterGroupActivationsAsync(false).ConfigureAwait(false);

                    // Assert
                    Assert.Equal(device, status.DeviceId);
                    Assert.Equal(module, status.ModuleId);
                    Assert.Equal(3, status.Entities.Count);
                    Assert.Equal(3, activations.Count);
                    Assert.Equal(4, includingNotConnected.Count);
                    Assert.Single(includingNotConnected.Where(e => e.ActivationState == EntityActivationState.Activated));
                    Assert.All(status.Entities, e => {
                        Assert.StartsWith("ua26029342304923", e.Id);
                        Assert.Equal(EntityActivationState.Activated, e.ActivationState);
                    });
                    Assert.All(activations, e => {
                        Assert.StartsWith("ua26029342304923", e.Id);
                        Assert.Equal(EntityActivationState.Activated, e.ActivationState);
                    });
                }).ConfigureAwait(false);
            }
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
#endif
    }
}
