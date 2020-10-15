// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Mock;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Autofac.Extras.Moq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Xunit;
    using Autofac;

    public class ApplicationRegistryTests {

        [Fact]
        public void GetApplicationThatDoesNotExistThrowsResourceNotFoundException() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {

                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var t = service.GetApplicationAsync("test", false);

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
                var result = await service.GetApplicationAsync(first.ApplicationId, false);

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
                var toUpdate = await service.FindApplicationAsync(appId);
                service.UpdateApplicationAsync(appId, new ApplicationInfoUpdateModel {
                    GenerationId = toUpdate.Application.GenerationId,
                    ApplicationName = "TestName",
                    DiscoveryProfileUri = "pu"
                }).Wait();

                var result = await service.GetApplicationAsync(appId, false);

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
                var records = await service.ListApplicationsAsync(null, null);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task ListAllApplicationsUsingQueryAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(null, null);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task QueryApplicationsByClientAndServerApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.ClientAndServer
                }, null);

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
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.Server
                }, null);

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType != ApplicationType.Client), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsByDiscoveryServerApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.DiscoveryServer
                }, null);

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType == ApplicationType.DiscoveryServer), records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryApplicationsBySupervisorIdAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    DiscovererId = discovererId
                }, null);

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }


        [Fact]
        public async Task QueryApplicationsByClientApplicationTypeAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.Client
                }, null);

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
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationName = apps.First().ApplicationName
                }, null);

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
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationName = apps.First().ApplicationName.ToUpperInvariant()
                }, null);

                // Assert
                Assert.True(records.Items.Count == 0);
            }
        }

        [Fact]
        public async Task QueryApplicationsByApplicationUriDifferentCaseAsync() {
            using (var mock = CreateMock(out var hubName, out var discovererId, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = await service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationUri = apps.First().ApplicationUri.ToUpperInvariant()
                }, null);

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

                // Run
                foreach (var app in apps) {
                    var record = await service.RegisterApplicationAsync(
                        app.ToRegistrationRequest());
                }

                // Assert
                var records = await service.ListApplicationsAsync(null, null);

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
                var delete = await service.ListAllApplicationsAsync();
                foreach (var app in delete) {
                    await service.UnregisterApplicationAsync(app.ApplicationId, app.GenerationId);
                }

                // Assert
                var records = await service.ListApplicationsAsync(null, null);

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
                    () => service.GetApplicationAsync(null, false)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.GetApplicationAsync("", false)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync("abc", false)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync(Guid.NewGuid().ToString(), false)).ConfigureAwait(false);
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
                .Without(x => x.NotSeenSince)
                .With(x => x.DiscovererId, discx)
                .CreateMany(10)
                .ToList();
            apps.ForEach(x => x.SetApplicationId());

            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<ItemContainerFactory>().As<IItemContainerFactory>();
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
