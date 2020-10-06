// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage.Default;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Mock;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Autofac;
    using Microsoft.Azure.IIoT.Platform.Directory.Models;

    public class DiscoveryProcessorTests {

        [Fact]
        public void ProcessDiscoveryWithNoResultsAndNoExistingApplications() {
            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, noApps: true)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                Assert.Single(ApplicationsIn(mock));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithAlreadyExistingApplications() {
            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                Assert.True(ApplicationsIn(mock).IsSameAs(existing));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithOneExistingApplication() {
            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, countDevices: 1)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                Assert.Single(ApplicationsIn(mock));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithDifferentDiscoverersSameSiteApplications() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            // Assert no changes

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, fixup: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                Assert.True(ApplicationsIn(mock).IsSameAs(existing));
            }
        }

        [Fact]
        public void ProcessOneDiscoveryWithDifferentDiscoverersFromExisting() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, fixup: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Found one item
                found = new List<DiscoveryEventModel> { found.First() };
                // Assert there is still the same content as originally

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                Assert.True(ApplicationsIn(mock).IsSameAs(existing));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithDifferentDiscoverersFromExistingWhenExistingDisabled() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, fixup: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                var inreg = ApplicationsIn(mock);
                Assert.False(inreg.IsSameAs(existing));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.Null(a.Application.NotSeenSince));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
            }
        }

        [Fact]
        public void ProcessOneDiscoveryWithDifferentDiscoverersFromExistingWhenExistingDisabled() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, fixup: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Found one app and endpoint
                found = new List<DiscoveryEventModel> { found.First() };


                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                var inreg = ApplicationsIn(mock);
                Assert.False(inreg.IsSameAs(existing));
                Assert.Equal(discoverer, inreg.First().Application.DiscovererId);
                Assert.Null(inreg.First().Application.NotSeenSince);
                Assert.Equal(discoverer, inreg.First().Endpoints[0].DiscovererId);
            }
        }

        [Fact]
        public void ProcessDiscoveryWithNoResultsWithDifferentDiscoverersFromExisting() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found, fixup: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                // Found nothing
                found = new List<DiscoveryEventModel>();
                // Assert there is still the same content as originally

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert

                Assert.True(ApplicationsIn(mock).IsSameAs(existing));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithNoResultsAndExisting() {
            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found)) {
                // Found nothing
                found = new List<DiscoveryEventModel>();

                // Assert there is still the same content as originally but now disabled
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                var inreg = ApplicationsIn(mock);
                Assert.True(inreg.IsSameAs(existing));
                Assert.All(inreg, a => Assert.NotNull(a.Application.NotSeenSince));
            }
        }

        [Fact]
        public void ProcessDiscoveryWithOneEndpointResultsAndExisting() {
            // All applications, but only one endpoint each is enabled

            using (var mock = Setup(out var discoverer, out var supervisor, out var existing,
                out var found)) {

                // Found single endpoints
                found = found
                    .GroupBy(a => a.Application.ApplicationId)
                    .Select(x => x.First()).ToList();

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                service.ProcessDiscoveryEventsAsync(discoverer, supervisor, new DiscoveryResultModel(), found).Wait();

                // Assert
                var inreg = ApplicationsIn(mock);
                Assert.True(inreg.IsSameAs(existing));
            }
        }

        /// <summary>
        /// Extract application registrations from registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        private static List<ApplicationRegistrationModel> ApplicationsIn(AutoMock mock) {
            IApplicationRegistry registry = mock.Create<ApplicationRegistry>();
            var apps = new List<ApplicationRegistrationModel>();
            var result = registry.QueryAllApplicationsAsync(null).Result;
            foreach (var app in result) {
                apps.Add(registry.GetApplicationAsync(app.ApplicationId).Result);
            }
            return apps;
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        /// <param name="discoverer"></param>
        /// <param name="supervisor"></param>
        /// <param name="existing"></param>
        /// <param name="found"></param>
        /// <param name="countDevices"></param>
        /// <param name="noApps"></param>
        /// <param name="fixup"></param>
        /// <returns></returns>
        private static AutoMock Setup(out string discoverer, out string supervisor,
            out List<ApplicationRegistrationModel> existing, out List<DiscoveryEventModel> found, 
            int countDevices = -1, bool noApps = false,
            Func<ApplicationRegistrationModel, ApplicationRegistrationModel> fixup = null) {
            var fix = new Fixture();

            // Create template applications and endpoints
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());

            var hub = fix.Create<string>();
            var gateway = fix.Create<string>();
            var module = fix.Create<string>();
            var discovererx = discoverer = HubResource.Format(hub, gateway, module);
            module = fix.Create<string>();
            var supervisorx = supervisor = HubResource.Format(hub, gateway, module);

            var template = fix
                .Build<ApplicationRegistrationModel>()
                .Without(x => x.Application)
                .Do(c => c.Application = fix
                    .Build<ApplicationInfoModel>()
                    .Without(x => x.NotSeenSince)
                    .With(x => x.DiscovererId, discovererx)
                    .Create())
                .Without(x => x.Endpoints)
                .Do(c => c.Endpoints = fix
                    .Build<EndpointInfoModel>()
                    .With(x => x.DiscovererId, discovererx)
                    .With(x => x.SupervisorId, supervisorx)
                    .CreateMany(5)
                    .ToList())
                .CreateMany(5)
                .ToList();
            template.ForEach(a =>
                a.Application.ApplicationId =
                    ApplicationInfoModelEx.CreateApplicationId(a.Application)
            );

            // Create discovery results from template
            var i = 0; var now = DateTime.UtcNow;
            found = template
                 .SelectMany(a => a.Endpoints.Select(
                     e => new DiscoveryEventModel {
                         Application = a.Application,
                         Endpoint = e,
                         Index = i++,
                         TimeStamp = now
                     }))
                 .ToList();

            // Clone and fixup existing applications as per test case
            existing = template
                .Select(e => e.Clone())
                .Select(fixup ?? (a => a))
                .ToList();
            // and fill registry with them...
            var appdevices = existing
                .Select(a => a.Application);
            if (countDevices != -1) {
                appdevices = appdevices.Take(countDevices);
            }

            var epdevices = existing
                .SelectMany(a => a.Endpoints
                    .Select(e => {
                        e.ApplicationId = a.Application.ApplicationId;
                        return e;
                    }));
            //   if (countDevices != -1) {
            //       epdevices = epdevices.Take(countDevices);
            //   }


            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<ApplicationDatabase>().As<IApplicationRepository>();
                builder.RegisterType<EndpointDatabase>().As<IEndpointRepository>();
                 builder.RegisterType<EndpointRegistry>().As<IEndpointBulkProcessor>();
                builder.RegisterType<ApplicationRegistry>().As<IApplicationBulkProcessor>();
            });

            if (noApps) {
                return mock;
            }

            IApplicationRepository repo1 = mock.Create<ApplicationDatabase>();
            foreach (var app in appdevices) {
                repo1.AddAsync(app);
            }
            IEndpointRepository repo2 = mock.Create<EndpointDatabase>();
            foreach (var ep in epdevices) {
                repo2.AddAsync(ep);
            }
            return mock;
        }
    }
}
