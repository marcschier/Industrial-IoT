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
    public class SupervisorRegistryTests {

        [Fact]
        public void GetSupervisorThatDoesNotExist() {
            CreateSupervisorFixtures(out var hubName, out var site,out var supervisors, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName, modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                ISupervisorRegistry service = mock.Create<SupervisorRegistry>();

                // Run
                var t = service.GetSupervisorAsync(HubResource.Format(hubName, "test", "test"));

                // Assert
                Assert.NotNull(t.Exception);
                Assert.IsType<AggregateException>(t.Exception);
                Assert.IsType<ResourceNotFoundException>(t.Exception.InnerException);
            }
        }

        [Fact]
        public void GetSupervisorThatExists() {
            CreateSupervisorFixtures(out var hubName, out var site,out var supervisors, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName,modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                ISupervisorRegistry service = mock.Create<SupervisorRegistry>();

                // Run
                var result = service.GetSupervisorAsync(supervisors.First().Id).Result;

                // Assert
                Assert.True(result.IsSameAs(supervisors.First()));
            }
        }

        [Fact]
        public void UpdateSupervisorThatExists() {
            CreateSupervisorFixtures(out var hubName, out var site,out var supervisors, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName,modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                ISupervisorRegistry service = mock.Create<SupervisorRegistry>();

                // Run
                service.UpdateSupervisorAsync(supervisors.First().Id, new SupervisorUpdateModel {
                    LogLevel = TraceLogLevel.Debug
                }).Wait();
                var result = service.GetSupervisorAsync(supervisors.First().Id).Result;


                // Assert
                Assert.Equal(TraceLogLevel.Debug, result.LogLevel);
            }
        }

        [Fact]
        public void ListAllSupervisors() {
            CreateSupervisorFixtures(out var hubName, out var site,out var supervisors, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName,modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                ISupervisorRegistry service = mock.Create<SupervisorRegistry>();

                // Run
                var records = service.ListSupervisorsAsync(null, null).Result;

                // Assert
                Assert.True(supervisors.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void ListAllSupervisorsUsingQuery() {
            CreateSupervisorFixtures(out var hubName, out var site,out var supervisors, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(hubName,modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                ISupervisorRegistry service = mock.Create<SupervisorRegistry>();

                // Run
                var records = service.QuerySupervisorsAsync(null, null).Result;

                // Assert
                Assert.True(supervisors.IsSameAs(records.Items));
            }
        }

        /// <summary>
        /// Helper to create app fixtures
        /// </summary>
        /// <param name="site"></param>
        /// <param name="supervisors"></param>
        /// <param name="modules"></param>
        private void CreateSupervisorFixtures(out string hub, out string site,
            out List<SupervisorModel> supervisors, out List<(DeviceTwinModel, DeviceModel)> modules,
            bool noSite = false) {
            var fix = new Fixture();
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());
            var hubx = hub = fix.Create<string>();
            var sitex = site = noSite ? null : fix.Create<string>();
            supervisors = fix
                .Build<SupervisorModel>()
                .Without(x => x.Id)
                .Do(x => x.Id = HubResource.Format(hubx,
                    fix.Create<string>(), fix.Create<string>()))
                .CreateMany(10)
                .ToList();

            modules = supervisors
                .Select(a => a.ToSupervisorRegistration())
                .Select(a => a.ToDeviceTwin(_serializer))
                .Select(t => {
                    t.Properties.Reported = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Supervisor
                    };
                    return t;
                })
                .Select(t => (t, new DeviceModel { Hub = t.Hub, Id = t.Id, ModuleId = t.ModuleId }))
                .ToList();
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
