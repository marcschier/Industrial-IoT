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

    public class EndpointRegistryTests {

        [Fact]
        public void GetTwinThatDoesNotExist() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var t = service.GetEndpointAsync("test");

                // Assert
                Assert.NotNull(t.Exception);
                Assert.IsType<AggregateException>(t.Exception);
                Assert.IsType<ResourceNotFoundException>(t.Exception.InnerException);
            }
        }

        [Fact]
        public void GetTwinThatExists() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var first = endpoints.First();
                var id = EndpointInfoModelEx.CreateEndpointId(first.ApplicationId,
                    first.EndpointUrl, first.Endpoint.SecurityMode,
                    first.Endpoint.SecurityPolicy);

                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var result = service.GetEndpointAsync(id).Result;

                // Assert
                Assert.True(result.IsSameAs(endpoints.First()));
            }
        }

        [Fact]
        public void ListAllTwins() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.ListEndpointsAsync(null, null).Result;

                // Assert
                Assert.True(endpoints.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void ListAllTwinsUsingQuery() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(null, null).Result;

                // Assert
                Assert.True(endpoints.IsSameAs(records.Items));
            }
        }

        [Fact]
        public void QueryTwinsBySignSecurityMode() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var count = endpoints.Count(x => x.Endpoint.SecurityMode == SecurityMode.Sign);
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityMode = SecurityMode.Sign
                }, null).Result;

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public void QueryTwinsByActivation() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var count = endpoints.Count(x => x.IsActivated());
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Activated = true
                }, null).Result;

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public void QueryTwinsByDeactivation() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var count = endpoints.Count(x => !x.IsActivated());
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Activated = false
                }, null).Result;

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public void QueryTwinsByConnectivity() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var count = endpoints.Count(x => x.IsConnected());
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Connected = true
                }, null).Result;

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public void QueryTwinsByDisconnectivity() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                var count = endpoints.Count(x => !x.IsConnected());
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Connected = false
                }, null).Result;

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public void QueryTwinsBySecurityPolicySameCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityPolicy = endpoints.First().Endpoint.SecurityPolicy
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(endpoints.First()));
            }
        }

        [Fact]
        public void QueryTwinsBySecurityPolicyDifferentCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityPolicy = endpoints.First().Endpoint.SecurityPolicy.ToUpperInvariant()
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count == 0);
            }
        }

        [Fact]
        public void QueryTwinsByEndpointUrlDifferentCase() {
            using (var mock = CreateMock(out var hubName, out var site, out var super, out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Url = endpoints.First().Endpoint.Url.ToUpperInvariant()
                }, null).Result;

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(endpoints.First()));
            }
        }
        public static AutoMock CreateMock(out string hubName, out string site, out string super,
            out List<EndpointInfoModel> endpoints, bool noSite = false, bool noAdd = false) {
            var fix = new Fixture();
            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());
            var sitex = site = noSite ? null : fix.Create<string>();
            var hubNamex = hubName = fix.Create<string>();
            var superx = super = HubResource.Format(hubNamex, fix.Create<string>(), null);
            endpoints = fix
                .Build<EndpointInfoModel>()
                .Without(x => x)
                .Do(x => x = fix
                    .Build<EndpointInfoModel>()
                    .With(y => y.SupervisorId, superx)
                    .Create())
                .CreateMany(10)
                .ToList();

            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<ItemContainerFactory>().As<IItemContainerFactory>();
                builder.RegisterType<EndpointDatabase>().As<IEndpointRepository>();
            });

            if (!noAdd) {
                IEndpointRepository repo = mock.Create<EndpointDatabase>();
                foreach (var endpoint in endpoints) {
                    repo.AddAsync(endpoint);
                }
            }
            return mock;
        }
    }
}
