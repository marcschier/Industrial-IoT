// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Services {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Storage;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Autofac;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class ApplicationRegistryTests {

        [Fact]
        public void GetApplicationThatDoesNotExistThrowsResourceNotFoundException() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {

                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var t = service.GetApplicationAsync("test");

                // Assert
                Assert.NotNull(t.Exception);
                Assert.IsType<AggregateException>(t.Exception);
                Assert.IsType<ResourceNotFoundException>(t.Exception.InnerException);
            }
        }

        [Fact]
        public async Task GetApplicationThatExistsAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                var first = apps.First();
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var result = await service.GetApplicationAsync(first.ApplicationId).ConfigureAwait(false);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Application);
                Assert.True(result.Application.IsSameAs(apps.First()));
                Assert.True(result.Endpoints.Count == 0);
            }
        }

        [Fact]
        public async Task UpdateApplicationThatExistsAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                var first = apps.First();
                var appId = first.ApplicationId;
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var toUpdate = await service.FindApplicationAsync(appId).ConfigureAwait(false);
                service.UpdateApplicationAsync(appId, new ApplicationInfoUpdateModel {
                    GenerationId = toUpdate.Application.GenerationId,
                    ApplicationName = "TestName",
                    DiscoveryProfileUri = "pu"
                }).Wait();

                var result = await service.GetApplicationAsync(appId).ConfigureAwait(false);

                // Assert
                Assert.Equal("TestName", result.Application.ApplicationName);
                Assert.Equal("pu", result.Application.DiscoveryProfileUri);
            }
        }

        [Fact]
        public async Task ListAllApplicationsAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.ListApplicationsAsync(null, null).ConfigureAwait(false);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task ListAllApplicationsUsingQueryAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(null, null).ConfigureAwait(false);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task QueryApplicationsByClientAndServerApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationType = ApplicationType.ClientAndServer
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(apps.Count(x =>
                    x.ApplicationType == ApplicationType.ClientAndServer), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsByServerApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationType = ApplicationType.Server
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType != ApplicationType.Client), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsByDiscoveryServerApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationType = ApplicationType.DiscoveryServer
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType == ApplicationType.DiscoveryServer), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsBySupervisorIdAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    DiscovererId = discovererId
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }


        [Fact]
        public async Task QueryApplicationsByClientApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationType = ApplicationType.Client
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(apps.Count(x =>
                    x.ApplicationType != ApplicationType.Server &&
                    x.ApplicationType != ApplicationType.DiscoveryServer), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsByApplicationNameSameCaseAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationName = apps.First().ApplicationName
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(apps.First()));
            }
        }

        [Fact]
        public async Task QueryApplicationsByApplicationNameDifferentCaseAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationName = apps.First().ApplicationName.ToUpperInvariant()
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count == 0);
            }
        }

        [Fact]
        public async Task QueryApplicationsByApplicationUriDifferentCaseAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationInfoQueryModel {
                    ApplicationUri = apps.First().ApplicationUri.ToUpperInvariant()
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(apps.First()));
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public async Task RegisterApplicationAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps, true)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();
                apps.ForEach(a => a.Visibility = EntityVisibility.Unknown);

                // Run
                foreach (var app in apps) {
                    var record = await service.RegisterApplicationAsync(
                        app.ToRegistrationRequest()).ConfigureAwait(false);
                }

                // Assert
                var records = await service.ListApplicationsAsync(null, null).ConfigureAwait(false);

                Assert.Equal(apps.Count, records.Items.Count);
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public async Task UnregisterApplicationsAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var delete = await service.ListAllApplicationsAsync().ConfigureAwait(false);
                foreach (var app in delete) {
                    await service.UnregisterApplicationAsync(app.ApplicationId, app.GenerationId).ConfigureAwait(false);
                }

                // Assert
                var records = await service.ListApplicationsAsync(null, null).ConfigureAwait(false);

                Assert.Empty(records.Items);
                Assert.Null(records.ContinuationToken);
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public async Task BadArgShouldThrowExceptionsAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.RegisterApplicationAsync(null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.GetApplicationAsync(null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.GetApplicationAsync("")).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync("abc")).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync(Guid.NewGuid().ToString())).ConfigureAwait(false);
            }
        }

        public static AutoMock CreateMock(out string hub, out string disc, out List<ApplicationInfoModel> apps,
            bool noAdd = false) {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlySet<>), typeof(HashSet<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyList<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyCollection<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var hubx = hub = fixture.Create<string>();
            var discx = disc = HubResource.Format(hubx, fixture.Create<string>(), null);
            apps = fixture
                .Build<ApplicationInfoModel>()
                // .Without(x => x.NotSeenSince)
                .With(x => x.DiscovererId, discx)
                .CreateMany(10)
                .ToList();
            apps.ForEach(x => x.SetApplicationId());

            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterGeneric(typeof(OptionsMock<>)).AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<CollectionFactory>().As<ICollectionFactory>();
                builder.RegisterType<ApplicationDatabase>().As<IApplicationRepository>();
            });

            if (!noAdd) {
                IApplicationRepository repo = mock.Create<ApplicationDatabase>();
                foreach (var app in apps) {
                    repo.AddAsync(app);
                }
            }
            return mock;
        }
    }
}
