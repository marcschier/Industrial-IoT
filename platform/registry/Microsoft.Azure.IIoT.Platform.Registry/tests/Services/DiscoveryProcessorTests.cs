// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Storage;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Autofac;
    using System.Threading.Tasks;

    public class DiscoveryProcessorTests {

        [Fact]
        public async Task ProcessDiscoveryWithNoResultsAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                discoveryResults = new List<DiscoveryResultModel>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.Empty(inreg);
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneResultAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults.Take(1));

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.Single(inreg);
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndAlreadyExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndOneExistingApplicationAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 1)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }

        [Fact]
        public async Task ProcessAllResultsWithDifferentDiscoverersFromExistingAsync() {
            var fix = new Fixture();
            var oldDiscovererId = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            // Assert no changes

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.DiscovererId = oldDiscovererId;
                    x.Endpoints.ForEach(e => e.DiscovererId = oldDiscovererId);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert

                // Assert that 5 new items was added and 5 old ones are in the database
                var inreg = await ListApplicationsAsync(mock);
                Assert.Equal(10, inreg.Count);
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));

                var oldItems = await ListApplicationsAsync(mock, discovererId: oldDiscovererId);
                Assert.Equal(5, oldItems.Count);
                Assert.All(oldItems, a => Assert.Equal(oldDiscovererId, a.Application.DiscovererId));
                Assert.All(oldItems, a => Assert.All(a.Endpoints, e => Assert.Equal(oldDiscovererId, e.DiscovererId)));

                var newItems = await ListApplicationsAsync(mock, discovererId: discoverer);
                Assert.True(newItems.IsSameAs(expected));
                Assert.All(newItems, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(newItems, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
            }
        }

        [Fact]
        public async Task ProcessOneResultWithDifferentDiscoverersFromExistingAsync() {
            var fix = new Fixture();
            var oldDiscovererId = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.DiscovererId = oldDiscovererId;
                    x.Endpoints.ForEach(e => e.DiscovererId = oldDiscovererId);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Found one item
                discoveryResults = new List<DiscoveryResultModel> { discoveryResults.First() };
                // Assert there is still the same content as originally

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert that one new item was added and 5 old ones are in the database
                var inreg = await ListApplicationsAsync(mock);
                Assert.Equal(6, inreg.Count);
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));

                var oldItems = await ListApplicationsAsync(mock, discovererId: oldDiscovererId);
                Assert.Equal(5, oldItems.Count);
                Assert.All(oldItems, a => Assert.Equal(oldDiscovererId, a.Application.DiscovererId));
                Assert.All(oldItems, a => Assert.All(a.Endpoints, e => Assert.Equal(oldDiscovererId, e.DiscovererId)));
                var newItems = await ListApplicationsAsync(mock, discovererId: discoverer);
                Assert.Single(newItems);
                Assert.All(newItems, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(newItems, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
            }
        }

        [Fact]
        public async Task ProcessAllResultsWhenExistingDisabledAsync() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.NotSeenSince = DateTime.UtcNow;
                    x.Endpoints.ForEach(e => {
                        e.NotSeenSince = DateTime.UtcNow;
                    });
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }

        [Fact]
        public async Task ProcessOneResultWhenExistingDisabledAsync() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.NotSeenSince = DateTime.UtcNow;
                    x.Endpoints.ForEach(e => {
                        e.NotSeenSince = DateTime.UtcNow;
                    });
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults.Take(1));

                // Assert that one item is enabled and 4 are still disabled
                var inreg = await ListApplicationsAsync(mock);
                Assert.Equal(5, inreg.Count);
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));

                var disabled = await ListApplicationsAsync(mock, includeNotSeenSince: false);
                disabled = disabled.Where(x => x.Application.NotSeenSince != null).ToList();
                Assert.Equal(4, disabled.Count);
                Assert.All(inreg, a => Assert.True(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.True(a.Application.IsDisabled())));
                var enabled = await ListApplicationsAsync(mock, includeNotSeenSince: false);
                Assert.Single(enabled);
                Assert.All(inreg, a => Assert.False(a.Application.IsDisabled()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsDisabled())));
            }
        }







        [Fact]
        public async Task ProcessOneDiscoveryWithDifferentDiscoverersFromExistingWhenExistingDisabledAsync() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.DiscovererId = discoverer2;
                    x.Endpoints.ForEach(e => e.DiscovererId = discoverer2);
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Found one app and endpoint
                discoveryResults = new List<DiscoveryResultModel> { discoveryResults.First() };


                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.False(inreg.IsSameAs(expected));
                Assert.Equal(discoverer, inreg.First().Application.DiscovererId);
                Assert.Null(inreg.First().Application.NotSeenSince);
                Assert.Equal(discoverer, inreg.First().Endpoints[0].DiscovererId);
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithNoResultsWillDisableExistingApplicationsAsync() {

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {

                // Found nothing
                discoveryResults = new List<DiscoveryResultModel>();

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert

                // Assert all applications and endpoints are disabled
                var inreg = await ListApplicationsAsync(mock);
                Assert.False(inreg.IsSameAs(expected));
                Assert.Equal(discoverer, inreg.First().Application.DiscovererId);
                Assert.True(inreg.First().Application.IsDisabled());
                Assert.Equal(discoverer, inreg.First().Endpoints[0].DiscovererId);
                Assert.True(inreg.First().Endpoints[0].IsDisabled());
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithNoResultsAndExistingAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {
                // Found nothing
                discoveryResults = new List<DiscoveryResultModel>();

                // Assert there is still the same content as originally but now disabled
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.False(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.True(a.Application.IsDisabled()));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneEndpointResultsAndExistingAsync() {
            // All applications, but only one endpoint each is enabled

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {

                // Found single endpoints
                discoveryResults = discoveryResults
                    .GroupBy(a => a.Application.ApplicationId)
                    .Select(x => x.First()).ToList();

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults);

                // Assert
                var inreg = await ListApplicationsAsync(mock);
                Assert.True(inreg.IsSameAs(expected));
            }
        }

        /// <summary>
        /// Extract application registrations from registry
        /// </summary>
        private static async Task<List<ApplicationRegistrationModel>> ListApplicationsAsync(AutoMock mock,
            bool includeNotSeenSince = true, string discovererId = null) {
            IApplicationRegistry registry = mock.Create<ApplicationRegistry>();
            var apps = new List<ApplicationRegistrationModel>();
            var result = await registry.QueryAllApplicationsAsync(new ApplicationRegistrationQueryModel {
                IncludeNotSeenSince = includeNotSeenSince,
                DiscovererId = discovererId
            });
            foreach (var app in result) {
                var reg = await registry.GetApplicationAsync(app.ApplicationId);
                apps.Add(reg);
            }
            return apps;
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        private static AutoMock Setup(out string discoverer, out List<ApplicationRegistrationModel> expectedState,
            out List<DiscoveryResultModel> discoveryResults, int existingEntries = -1, 
            Func<ApplicationRegistrationModel, ApplicationRegistrationModel> fixupDatabase = null) {

            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlySet<>), typeof(HashSet<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyList<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyCollection<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Create template applications and endpoints

            var hub = fixture.Create<string>();
            var gateway = fixture.Create<string>();
            var module = fixture.Create<string>();
            var discovererx = discoverer = HubResource.Format(hub, gateway, module);

            var template = fixture
                .Build<ApplicationRegistrationModel>()
                .Without(x => x.Application)
                .Do(c => c.Application = fixture
                    .Build<ApplicationInfoModel>()
                    .Without(x => x.NotSeenSince)
                    .With(x => x.DiscovererId, discovererx)
                    .Create())
                .Without(x => x.Endpoints)
                .Do(c => c.Endpoints = fixture
                    .Build<EndpointInfoModel>()
                    .With(x => x.DiscovererId, discovererx)
                    .Without(x => x.NotSeenSince)
                    .CreateMany(5)
                    .ToList())
                .CreateMany(5)
                .ToList();

            // Create discovery results from template
            var i = 0; var now = DateTime.UtcNow;
            discoveryResults = template
                 .SelectMany(a => a.Endpoints.Select(
                     e => new DiscoveryResultModel {
                         Application = a.Application.Clone(),
                         Endpoint = e.Clone(),
                         Index = i++,
                         TimeStamp = now
                     }))
                 .ToList();

            // Clone and fixup expected applications as per test case
            expectedState = template
                .Select(app => app.Clone())
                .ToList();
            expectedState.ForEach(a => {
                a.Application.DiscovererId = discovererx;
                a.Application.SetApplicationId();
                a.Endpoints.ToList().ForEach(e => {
                    e.ApplicationId = a.Application.ApplicationId;
                    e.DiscovererId = discovererx;
                    e.SetEndpointId();
                });
            });

            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<ItemContainerFactory>().As<IItemContainerFactory>();
                builder.RegisterType<ApplicationDatabase>().As<IApplicationRepository>();
                builder.RegisterType<EndpointDatabase>().As<IEndpointRepository>();
                builder.RegisterType<EndpointRegistry>().AsImplementedInterfaces();
                builder.RegisterType<ApplicationRegistry>().AsImplementedInterfaces();
            });

            if (existingEntries != 0) {

                var initialState = expectedState
                    .Select(app => app.Clone())
                    .Select(fixupDatabase ?? (a => a));
                if (existingEntries != -1) {
                    initialState = initialState.Take(existingEntries);
                }

                // and fill database with application and endpoints...
                IApplicationRepository appDatabase = mock.Create<ApplicationDatabase>();
                IEndpointRepository epDatabase = mock.Create<EndpointDatabase>();
                foreach (var app in initialState) {
                    appDatabase.AddAsync(app.Application);
                    foreach (var ep in app.Endpoints) {
                        epDatabase.AddAsync(ep);
                    }
                }
            }
            return mock;
        }
    }
}
