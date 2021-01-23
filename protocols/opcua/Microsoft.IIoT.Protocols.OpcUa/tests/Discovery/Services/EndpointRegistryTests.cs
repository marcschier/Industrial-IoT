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
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers;
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
    public class EndpointRegistryTests {

        [Fact]
        public void GetTwinThatDoesNotExistThrowsResourceNotFoundException() {
            using (var mock = CreateMock(out var endpoints)) {
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
        public async Task GetTwinThatExistsAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                var first = endpoints.First();

                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var result = await service.GetEndpointAsync(first.Id).ConfigureAwait(false);

                // Assert
                Assert.True(result.IsSameAs(endpoints.First()));
            }
        }

        [Fact]
        public async Task ListAllEndpointsAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.ListEndpointsAsync(null, null).ConfigureAwait(false);

                // Assert
                Assert.True(endpoints.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task ListAllEndpointsUsingQueryAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(null, null).ConfigureAwait(false);

                // Assert
                Assert.True(endpoints.IsSameAs(records.Items));
            }
        }

        [Fact]
        public async Task QueryEndpointsBySignSecurityModeAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                var count = endpoints.Count(x => x.Endpoint.SecurityMode == SecurityMode.Sign);
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityMode = SecurityMode.Sign
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

#if FALSE
        [Fact]
        public async Task QueryEndpointsByConnectivityAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                var count = endpoints.Count(x => x.EndpointState == ConnectionStatus.Ready);
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    EndpointState = ConnectionStatus.Ready
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }

        [Fact]
        public async Task QueryEndpointsByDisconnectivityAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                var count = endpoints.Count(x => x.EndpointState == ConnectionStatus.Disconnected);
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    EndpointState = ConnectionStatus.Disconnected
                }, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(count, records.Items.Count);
            }
        }
#endif

        [Fact]
        public async Task QueryEndpointsBySecurityPolicySameCaseAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityPolicy = endpoints.First().Endpoint.SecurityPolicy
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(endpoints.First()));
            }
        }

        [Fact]
        public async Task QueryEndpointsBySecurityPolicyDifferentCaseAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    SecurityPolicy = endpoints.First().Endpoint.SecurityPolicy.ToUpperInvariant()
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count == 0);
            }
        }

        [Fact]
        public async Task QueryEndpointsByEndpointUrlDifferentCaseAsync() {
            using (var mock = CreateMock(out var endpoints)) {
                IEndpointRegistry service = mock.Create<EndpointRegistry>();

                // Run
                var records = await service.QueryEndpointsAsync(new EndpointInfoQueryModel {
                    Url = endpoints.First().Endpoint.Url.ToUpperInvariant()
                }, null).ConfigureAwait(false);

                // Assert
                Assert.True(records.Items.Count >= 1);
                Assert.True(records.Items[0].IsSameAs(endpoints.First()));
            }
        }
        public static AutoMock CreateMock(out List<EndpointInfoModel> endpoints, bool noAdd = false) {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlySet<>), typeof(HashSet<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyList<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>)));
            fixture.Customizations.Add(new TypeRelay(typeof(IReadOnlyCollection<>), typeof(List<>)));
            fixture.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            endpoints = fixture
                .Build<EndpointInfoModel>()
                .Do(x => x = fixture
                    .Build<EndpointInfoModel>()
                    .Create())
                .Without(x => x.NotSeenSince)
                .With(x => x.Visibility, EntityVisibility.Found)
                .CreateMany(10)
                .ToList();

            endpoints.ForEach(x => x.SetEndpointId());

            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterGeneric(typeof(OptionsMock<>)).AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<CollectionFactory>().As<ICollectionFactory>();
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
