// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Services {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Azure.IoTHub.Testing;
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Serializers;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class DiscovererRegistryTests {

        [Fact]
        public void GetDiscovererThatDoesNotExist() {
            CreateDiscovererFixtures(out var hubName, out _, out _, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IDiscovererRegistry service = mock.Create<DiscovererRegistry>();

                // Run
                var t = service.GetDiscovererAsync(HubResource.Format(hubName, "test", "test"));

                // Assert
                Assert.NotNull(t.Exception);
                Assert.IsType<AggregateException>(t.Exception);
                Assert.IsType<ResourceNotFoundException>(t.Exception.InnerException);
            }
        }

        [Fact]
        public void GetDiscovererThatExists() {
            CreateDiscovererFixtures(out var hubName, out _, out var discoverers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IDiscovererRegistry service = mock.Create<DiscovererRegistry>();

                // Run
                var result = service.GetDiscovererAsync(discoverers.First().Id).Result;

                // Assert
                Assert.True(result.IsSameAs(discoverers.First()));
            }
        }

        [Fact]
        public void UpdateDiscovererThatExists() {
            CreateDiscovererFixtures(out var hubName, out var site, out var discoverers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IDiscovererRegistry service = mock.Create<DiscovererRegistry>();

                // Run
                service.UpdateDiscovererAsync(discoverers.First().Id, new DiscovererUpdateModel {
                    LogLevel = TraceLogLevel.Debug
                }).Wait();
                var result = service.GetDiscovererAsync(discoverers.First().Id).Result;


                // Assert
                Assert.Equal(TraceLogLevel.Debug, result.LogLevel);
            }
        }

        [Fact]
        public void ListAllDiscoverers() {
            CreateDiscovererFixtures(out var hubName, out _, out var discoverers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IDiscovererRegistry service = mock.Create<DiscovererRegistry>();

                // Run
                var records = service.ListDiscoverersAsync(null, null).Result;

                // Assert
                Assert.True(discoverers.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void ListAllDiscoverersUsingQuery() {
            CreateDiscovererFixtures(out var hubName, out _, out var discoverers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IDiscovererRegistry service = mock.Create<DiscovererRegistry>();

                // Run
                var records = service.QueryDiscoverersAsync(null, null).Result;

                // Assert
                Assert.True(discoverers.IsSameAs(records.Items));
            }
        }

        /// <summary>
        /// Helper to create app fixtures
        /// </summary>
        /// <param name="site"></param>
        /// <param name="discoverers"></param>
        /// <param name="modules"></param>
        private void CreateDiscovererFixtures(out string hubName, out string site,
            out List<DiscovererModel> discoverers,
            out List<(DeviceTwinModel, DeviceModel)> modules,
            bool noSite = false) {
            var fix = new Fixture();
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());
            var sitex = site = noSite ? null : fix.Create<string>();
            var hubx = hubName = fix.Create<string>();
            discoverers = fix
                .Build<DiscovererModel>()
                .Without(x => x.Id)
                .Do(x => x.Id = HubResource.Format(hubx,
                    fix.Create<string>(), fix.Create<string>()))
                .CreateMany(10)
                .ToList();

            modules = discoverers
                .Select(a => {
                    var r = a.ToDiscovererRegistration();
                    r._desired = r;
                    return r;
                })
                .Select(a => a.ToDeviceTwin(_serializer))
                .Select(t => {
                    t.Properties.Reported = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Discoverer
                    };
                    return t;
                })
                .Select(t => (t, new DeviceModel { Hub = t.Hub, Id = t.Id, ModuleId = t.ModuleId }))
                .ToList();
        }
        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
