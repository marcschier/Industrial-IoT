// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage.Default;
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
        public void GetApplicationThatDoesNotExist() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {

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
        public void GetApplicationThatExists() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                var first = apps.First();
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var result = service.GetApplicationAsync(
                    ApplicationInfoModelEx.CreateApplicationId(site,
                    first.ApplicationUri, first.ApplicationType), false).Result;

                // Assert
                Assert.True(result.Application.IsSameAs(apps.First()));
                Assert.True(result.Endpoints.Count == 0);
            }
        }

        [Fact]
        public void UpdateApplicationThatExists() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                var first = apps.First();
                var appId = ApplicationInfoModelEx.CreateApplicationId(site, first.ApplicationUri,
                    first.ApplicationType);
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                service.UpdateApplicationAsync(appId, new ApplicationInfoUpdateModel {
                    ApplicationName = "TestName",
                    DiscoveryProfileUri = "pu"
                }).Wait();

                var result = service.GetApplicationAsync(appId, false).Result;

                // Assert
                Assert.Equal("TestName", result.Application.ApplicationName);
                Assert.Equal("pu", result.Application.DiscoveryProfileUri);
            }
        }

        [Fact]
        public void ListAllApplications() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.ListApplicationsAsync(null, null).Result;

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void ListAllApplicationsUsingQuery() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(null, null).Result;

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void QueryApplicationsByClientAndServerApplicationType() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.ClientAndServer
                }, null).Result;

                // Assert
                Assert.Equal(apps.Count(x =>
                    x.ApplicationType == ApplicationType.ClientAndServer), records.Items.Count);
            }
        }

        [Fact]
        public void QueryApplicationsByServerApplicationType() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.Server
                }, null).Result;

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType != ApplicationType.Client), records.Items.Count);
            }
        }

        [Fact]
        public void QueryApplicationsByDiscoveryServerApplicationType() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.DiscoveryServer
                }, null).Result;

                // Assert
                Assert.Equal(apps.Count(x => x.ApplicationType == ApplicationType.DiscoveryServer), records.Items.Count);
            }
        }

        [Fact]
        public void QueryApplicationsBySiteId() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    SiteOrGatewayId = site
                }, null).Result;

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void QueryApplicationsBySupervisorId() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    SiteOrGatewayId = HubResource.Parse(super, out _, out _)
                }, null).Result;

                // Assert
                Assert.True(apps.IsSameAs(records.Items));
            }
        }


        [Fact]
        public void QueryApplicationsByClientApplicationType() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationType = ApplicationType.Client
                }, null).Result;

                // Assert
                Assert.Equal(apps.Count(x =>
                    x.ApplicationType != ApplicationType.Server &&
                    x.ApplicationType != ApplicationType.DiscoveryServer), records.Items.Count);
            }
        }

        [Fact]
        public void QueryApplicationsByApplicationNameSameCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationName = apps.First().ApplicationName
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items.First().IsSameAs(apps.First()));
            }
        }

        [Fact]
        public void QueryApplicationsByApplicationNameDifferentCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationName = apps.First().ApplicationName.ToUpperInvariant()
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count == 0);
            }
        }

        [Fact]
        public void QueryApplicationsByApplicationUriDifferentCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                var records = service.QueryApplicationsAsync(new ApplicationRegistrationQueryModel {
                    ApplicationUri = apps.First().ApplicationUri.ToUpperInvariant()
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items.First().IsSameAs(apps.First()));
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public void RegisterApplication() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps,
                false, true)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                foreach (var app in apps) {
                    var record = service.RegisterApplicationAsync(
                        app.ToRegistrationRequest()).Result;
                }

                // Assert
                var records = service.ListApplicationsAsync(null, null).Result;

                Assert.Equal(apps.Count, records.Items.Count);
                Assert.True(apps.IsSameAs(records.Items));
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public void UnregisterApplications() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                // Run
                foreach (var app in apps) {
                    service.UnregisterApplicationAsync(app.ApplicationId, null).Wait();
                }

                // Assert
                var records = service.ListApplicationsAsync(null, null).Result;

                Assert.Empty(records.Items);
                Assert.Null(records.ContinuationToken);
            }
        }

        /// <summary>
        /// Test to register all applications in the test set.
        /// </summary>
        [Fact]
        public async Task BadArgShouldThrowExceptionsAsync() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var apps)) {
                IApplicationRegistry service = mock.Create<ApplicationRegistry>();

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.RegisterApplicationAsync(null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.GetApplicationAsync(null, false));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => service.GetApplicationAsync("", false));
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync("abc", false));
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => service.GetApplicationAsync(Guid.NewGuid().ToString(), false));
            }
        }

        public AutoMock CreateMock(out string hub, out string site, out string super,
            out List<ApplicationInfoModel> apps, bool noSite = false, bool noAdd = false) {
            var fix = new Fixture();
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());
            var sitex = site = noSite ? null : fix.Create<string>();
            var hubx = hub = fix.Create<string>();
            var superx = super = HubResource.Format(hubx, fix.Create<string>(), null);
            apps = fix
                .Build<ApplicationInfoModel>()
                .Without(x => x.NotSeenSince)
                .With(x => x.SiteId, sitex)
                .With(x => x.DiscovererId, superx)
                .CreateMany(10)
                .ToList();
            apps.ForEach(x => x.ApplicationId = ApplicationInfoModelEx.CreateApplicationId(
                 sitex, x.ApplicationUri, x.ApplicationType));

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

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
