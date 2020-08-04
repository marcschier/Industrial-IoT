// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Mock;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Autofac;

    public class PublisherRegistryTests {

        [Fact]
        public void GetPublisherThatDoesNotExist() {
            CreatePublisherFixtures(out var site, out var publishers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IPublisherRegistry service = mock.Create<PublisherRegistry>();

                // Run
                var t = service.GetPublisherAsync("test", false);

                // Assert
                Assert.NotNull(t.Exception);
                Assert.IsType<AggregateException>(t.Exception);
                Assert.IsType<ResourceNotFoundException>(t.Exception.InnerException);
            }
        }

        [Fact]
        public void GetPublisherThatExists() {
            CreatePublisherFixtures(out var site, out var publishers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IPublisherRegistry service = mock.Create<PublisherRegistry>();

                // Run
                var result = service.GetPublisherAsync(publishers.First().Id, false).Result;

                // Assert
                Assert.True(result.IsSameAs(publishers.First()));
            }
        }

        [Fact]
        public void UpdatePublisherThatExists() {
            CreatePublisherFixtures(out var site, out var publishers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IPublisherRegistry service = mock.Create<PublisherRegistry>();

                // Run
                service.UpdatePublisherAsync(publishers.First().Id, new PublisherUpdateModel {
                    LogLevel = TraceLogLevel.Debug
                }).Wait();
                var result = service.GetPublisherAsync(publishers.First().Id, false).Result;


                // Assert
                Assert.Equal(TraceLogLevel.Debug, result.LogLevel);
            }
        }

        [Fact]
        public void ListAllPublishers() {
            CreatePublisherFixtures(out var site, out var publishers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IPublisherRegistry service = mock.Create<PublisherRegistry>();

                // Run
                var records = service.ListPublishersAsync(null, false, null).Result;

                // Assert
                Assert.True(publishers.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void ListAllPublishersUsingQuery() {
            CreatePublisherFixtures(out var site, out var publishers, out var modules);

            using (var mock = AutoMock.GetLoose(builder => {
                var hub = IoTHubServices.Create(modules);
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterInstance(hub).As<IDeviceTwinServices>();
            })) {
                IPublisherRegistry service = mock.Create<PublisherRegistry>();

                // Run
                var records = service.QueryPublishersAsync(null, false, null).Result;

                // Assert
                Assert.True(publishers.IsSameAs(records.Items));
            }
        }

        /// <summary>
        /// Helper to create app fixtures
        /// </summary>
        /// <param name="site"></param>
        /// <param name="publishers"></param>
        /// <param name="modules"></param>
        private void CreatePublisherFixtures(out string site,
            out List<PublisherModel> publishers, out List<(DeviceTwinModel, DeviceModel)> modules,
            bool noSite = false) {
            var fix = new Fixture();
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());
            var sitex = site = noSite ? null : fix.Create<string>();
            publishers = fix
                .Build<PublisherModel>()
                .Without(x => x.Id)
                .Do(x => x.Id = PublisherModelEx.CreatePublisherId(
                    fix.Create<string>(), fix.Create<string>()))
                .CreateMany(10)
                .ToList();

            modules = publishers
                .Select(a => a.ToPublisherRegistration())
                .Select(a => a.ToDeviceTwin(_serializer))
                .Select(t => {
                    t.Properties.Reported = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Publisher
                    };
                    return t;
                })
                .Select(t => (t, new DeviceModel { Id = t.Id, ModuleId = t.ModuleId }))
                .ToList();
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
