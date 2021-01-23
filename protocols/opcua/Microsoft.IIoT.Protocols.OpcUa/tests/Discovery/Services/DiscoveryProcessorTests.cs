// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Services {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Storage;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.IIoT.Extensions.Utils;
    using Autofac;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class DiscoveryProcessorTests {

        [Fact]
        public async Task ProcessDiscoveryWithNoResultsAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                discoveryResults = new List<DiscoveryResultModel>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Empty(inreg);
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneResultAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults.Take(1)).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Single(inreg);
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndNoExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 0)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndAlreadyExistingApplicationsAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsAndOneExistingApplicationAsync() {
            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults, existingEntries: 1)) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsWithDifferentDiscoverersFromExistingAsync() {
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
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert

                // Assert that 5 new items was added and 5 old ones are in the database
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Equal(10, inreg.Count);
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));

                var oldItems = await ListApplicationsAsync(mock, discovererId: oldDiscovererId).ConfigureAwait(false);
                Assert.Equal(5, oldItems.Count);
                Assert.All(oldItems, a => Assert.Equal(oldDiscovererId, a.Application.DiscovererId));
                Assert.All(oldItems, a => Assert.All(a.Endpoints, e => Assert.Equal(oldDiscovererId, e.DiscovererId)));

                var newItems = await ListApplicationsAsync(mock, discovererId: discoverer).ConfigureAwait(false);
                Assert.True(newItems.IsSameAs(expected));
                Assert.All(newItems, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(newItems, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneResultWithDifferentDiscoverersFromExistingAsync() {
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
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert that one new item was added and 5 old ones are in the database
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Equal(6, inreg.Count);
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));

                var oldItems = await ListApplicationsAsync(mock, discovererId: oldDiscovererId).ConfigureAwait(false);
                Assert.Equal(5, oldItems.Count);
                Assert.All(oldItems, a => Assert.Equal(oldDiscovererId, a.Application.DiscovererId));
                Assert.All(oldItems, a => Assert.All(a.Endpoints, e => Assert.Equal(oldDiscovererId, e.DiscovererId)));
                var newItems = await ListApplicationsAsync(mock, discovererId: discoverer).ConfigureAwait(false);
                Assert.Single(newItems);
                Assert.All(newItems, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(newItems, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithAllResultsWhenExistingNotFoundAsync() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.SetAsLost();
                    x.Endpoints.ForEach(e => e.SetAsLost());
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.True(inreg.IsSameAs(expected));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneResultWhenExistingNotFoundAsync() {
            var fix = new Fixture();
            var discoverer2 = HubResource.Format(fix.Create<string>(), fix.Create<string>(), fix.Create<string>());

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults,
                fixupDatabase: x => {
                    x.Application.SetAsLost();
                    x.Endpoints.ForEach(e => e.SetAsLost());
                    return x;
                })) {
                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults.Take(1)).ConfigureAwait(false);

                // Assert that one item is enabled and 4 are still not found
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Equal(5, inreg.Count);
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));

                var notSeen = await ListApplicationsAsync(mock, visibilty: EntityVisibility.Lost).ConfigureAwait(false);
                Assert.Equal(4, notSeen.Count);
                Assert.All(notSeen, a => Assert.True(a.Application.IsLost()));
                Assert.All(notSeen, a => Assert.All(a.Endpoints, e => Assert.True(a.Application.IsLost())));
                var found = await ListApplicationsAsync(mock, visibilty: EntityVisibility.Found).ConfigureAwait(false);
                Assert.Single(found);
                Assert.All(found, a => Assert.False(a.Application.IsLost()));
                Assert.All(found, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithNoResultsWillDisableExistingApplicationsAsync() {

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {

                // Found nothing
                discoveryResults = new List<DiscoveryResultModel>();

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert

                // Assert all applications and endpoints were not found
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.Equal(5, inreg.Count);
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.True(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.True(a.Application.IsLost())));
                Assert.False(inreg.IsSameAs(expected));
            }
        }

        [Fact]
        public async Task ProcessDiscoveryWithOneEndpointResultsAndExistingAsync() {

            using (var mock = Setup(out var discoverer, out var expected, out var discoveryResults)) {

                // Found single endpoints
                discoveryResults = discoveryResults
                    .GroupBy(a => a.Application.ApplicationId)
                    .Select(x => x.First()).ToList();

                var service = mock.Create<IApplicationBulkProcessor>();

                // Run
                await service.ProcessDiscoveryEventsAsync(discoverer, new DiscoveryContextModel(), discoveryResults).ConfigureAwait(false);

                // Assert
                // All applications, but only one endpoint each is enabled
                var inreg = await ListApplicationsAsync(mock).ConfigureAwait(false);
                Assert.True(inreg.Select(a => a.Application).IsSameAs(expected.Select(b => b.Application)));
                Assert.All(inreg, a => Assert.Equal(discoverer, a.Application.DiscovererId));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.Equal(discoverer, e.DiscovererId)));
                Assert.All(inreg, a => Assert.False(a.Application.IsLost()));
                Assert.All(inreg, a => Assert.Single(a.Endpoints));
                Assert.All(inreg, a => Assert.All(a.Endpoints, e => Assert.False(a.Application.IsLost())));
            }
        }

        /// <summary>
        /// Extract application registrations from registry
        /// </summary>
        private static async Task<List<ApplicationRegistrationModel>> ListApplicationsAsync(AutoMock mock,
            EntityVisibility? visibilty = null, string discovererId = null) {
            IApplicationRegistry registry = mock.Create<ApplicationRegistry>();
            var apps = new List<ApplicationRegistrationModel>();
            var result = await registry.QueryAllApplicationsAsync(new ApplicationInfoQueryModel {
                Visibility = visibilty,
                DiscovererId = discovererId
            }).ConfigureAwait(false);
            foreach (var app in result) {
                var reg = await registry.GetApplicationAsync(app.ApplicationId).ConfigureAwait(false);
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
                    .With(x => x.Visibility, EntityVisibility.Found)
                    .With(x => x.DiscovererId, discovererx)
                    .Create())
                .Without(x => x.Endpoints)
                .Do(c => c.Endpoints = fixture
                    .Build<EndpointInfoModel>()
                    .With(x => x.DiscovererId, discovererx)
                    .Without(x => x.NotSeenSince)
                    .With(x => x.Visibility, EntityVisibility.Found)
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
                builder.RegisterGeneric(typeof(OptionsMock<>)).AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<CollectionFactory>().As<ICollectionFactory>();
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
