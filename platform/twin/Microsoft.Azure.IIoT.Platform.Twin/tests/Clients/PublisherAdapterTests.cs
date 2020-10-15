// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Services;
    using Microsoft.Azure.IIoT.Platform.Publisher.Storage.Services;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Clients;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Moq;
    using Autofac.Extras.Moq;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Autofac;

    public class PublisherAdapterTests {

        [Fact]
        public async Task StartPublishTest1Async() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Single(list.Items);
                Assert.Null(list.ContinuationToken);
                Assert.Equal("i=2258", list.Items.Single().NodeId);
                Assert.Equal(TimeSpan.FromSeconds(2), list.Items.Single().PublishingInterval);
                Assert.Equal(TimeSpan.FromSeconds(1), list.Items.Single().SamplingInterval);
            }
        }

        [Fact]
        public async Task StartPublishTest2Async() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258"
                    }
                }).ConfigureAwait(false);

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Single(list.Items);
                Assert.Null(list.ContinuationToken);
                Assert.Equal("i=2258", list.Items.Single().NodeId);
                Assert.Null(list.Items.Single().PublishingInterval);
                Assert.Null(list.Items.Single().SamplingInterval);
            }
        }

        [Fact]
        public async Task StartStopPublishTestAsync() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);

                var result2 = await service.NodePublishStopAsync("endpoint1", new PublishStopRequestModel {
                    NodeId = "i=2258"
                }).ConfigureAwait(false);

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.NotNull(result2);
                Assert.Empty(list.Items);
                Assert.Null(list.ContinuationToken);
            }
        }

        [Fact]
        public async Task StartTwicePublishTest1Async() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);
                result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);


                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Single(list.Items);
                Assert.Null(list.ContinuationToken);
                Assert.Equal("i=2258", list.Items.Single().NodeId);
                Assert.Equal(TimeSpan.FromSeconds(2), list.Items.Single().PublishingInterval);
                Assert.Equal(TimeSpan.FromSeconds(1), list.Items.Single().SamplingInterval);
            }
        }

        [Fact]
        public async Task StartTwicePublishTest2Async() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);
                result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2259",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);


                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Collection(list.Items,
                    a => {
                        Assert.Equal("i=2258", a.NodeId);
                        Assert.Equal(TimeSpan.FromSeconds(2), a.PublishingInterval);
                        Assert.Equal(TimeSpan.FromSeconds(1), a.SamplingInterval);
                    },
                    b => {
                        Assert.Equal("i=2259", b.NodeId);
                        Assert.Equal(TimeSpan.FromSeconds(2), b.PublishingInterval);
                        Assert.Equal(TimeSpan.FromSeconds(1), b.SamplingInterval);
                    });
                Assert.Null(list.ContinuationToken);
            }
        }

        [Fact]
        public async Task StartPublishSameNodeWithDifferentCredentialsOnlyHasLastInListAsync() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258"
                    },
                    Header = new RequestHeaderModel {
                        Elevation = new CredentialModel {
                            Type = CredentialType.UserName,
                            Value = "abcdefg"
                        }
                    }
                }).ConfigureAwait(false);
                result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258"
                    },
                    Header = new RequestHeaderModel {
                        Elevation = new CredentialModel {
                            Type = CredentialType.UserName,
                            Value = "123456"
                        }
                    }
                }).ConfigureAwait(false);
                result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258"
                    },
                    Header = new RequestHeaderModel {
                        Elevation = new CredentialModel {
                            Type = CredentialType.UserName,
                            Value = "asdfasdf"
                        }
                    }
                }).ConfigureAwait(false);


                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Single(list.Items);
                Assert.Null(list.ContinuationToken);
                Assert.Equal("i=2258", list.Items.Single().NodeId);
            }
        }

        [Fact]
        public async Task StartandStopPublishNodeWithDifferentCredentialsHasNoItemsInListAsync() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result1 = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258"
                    },
                    Header = new RequestHeaderModel {
                        Elevation = new CredentialModel {
                            Type = CredentialType.UserName,
                            Value = "abcdefg"
                        }
                    }
                }).ConfigureAwait(false);
                var result2 = await service.NodePublishStopAsync("endpoint1", new PublishStopRequestModel {
                    NodeId = "i=2258",
                    Header = new RequestHeaderModel {
                        Elevation = new CredentialModel {
                            Type = CredentialType.UserName,
                            Value = "123456"
                        }
                    }
                }).ConfigureAwait(false);

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.Empty(list.Items);
                Assert.Null(list.ContinuationToken);
            }
        }

        [Fact]
        public async Task StartTwicePublishTest3Async() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(2),
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }
                }).ConfigureAwait(false);
                result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                    Item = new PublishedItemModel {
                        NodeId = "i=2258",
                        PublishingInterval = TimeSpan.FromSeconds(3),
                        SamplingInterval = TimeSpan.FromSeconds(2)
                    }
                }).ConfigureAwait(false);

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.NotNull(result);
                Assert.Single(list.Items);
                Assert.Null(list.ContinuationToken);
                Assert.Equal("i=2258", list.Items.Single().NodeId);
                Assert.Equal(TimeSpan.FromSeconds(3), list.Items.Single().PublishingInterval);
                Assert.Equal(TimeSpan.FromSeconds(2), list.Items.Single().SamplingInterval);
            }
        }

        [Fact]
        public async Task StartStopMultiplePublishTestAsync() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                // Run
                for (var i = 0; i < 100; i++) {
                    var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                        Item = new PublishedItemModel {
                            NodeId = "i=" + (i + 1000),
                            PublishingInterval = TimeSpan.FromSeconds(i),
                            SamplingInterval = TimeSpan.FromSeconds(i+1)
                        }
                    }).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
                for (var i = 0; i < 50; i++) {
                    var result = await service.NodePublishStopAsync("endpoint1", new PublishStopRequestModel {
                        NodeId = "i=" + (i + 1000)
                    }).ConfigureAwait(false);
                    Assert.NotNull(result);
                }

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.Equal(50, list.Items.Count);
                Assert.Null(list.ContinuationToken);

                // Run
                for (var i = 0; i < 100; i++) {
                    var result = await service.NodePublishStartAsync("endpoint1", new PublishStartRequestModel {
                        Item = new PublishedItemModel {
                            NodeId = "i=" + (i + 2000),
                            PublishingInterval = TimeSpan.FromSeconds(i),
                            SamplingInterval = TimeSpan.FromSeconds(i + 1)
                        }
                    }).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
                for (var i = 0; i < 50; i++) {
                    var result = await service.NodePublishStopAsync("endpoint1", new PublishStopRequestModel {
                        NodeId = "i=" + (i + 2000)
                    }).ConfigureAwait(false);
                    Assert.NotNull(result);
                }

                list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.Equal(100, list.Items.Count);
                Assert.Null(list.ContinuationToken);
            }
        }

        [Fact]
        public async Task ListNodesWhenNoNodesConfiguredTestAsync() {

            using (var mock = Setup()) {

                IPublishServices service = mock.Create<PublishServicesAdapter>();

                var list = await service.NodePublishListAsync("endpoint1", new PublishedItemListRequestModel {
                    ContinuationToken = null
                }).ConfigureAwait(false);

                // Assert
                Assert.NotNull(list);
                Assert.Empty(list.Items);
                Assert.Null(list.ContinuationToken);
            }
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        /// <param name="mock"></param>
        /// <param name="provider"></param>
        private static AutoMock Setup() {
            var mock = AutoMock.GetLoose(builder => {

                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().SingleInstance().As<IDatabaseServer>();
                builder.RegisterType<MockConfig>().As<IItemContainerConfig>();
                var registry = new Mock<IEndpointRegistry>();
                registry
                    .Setup(e => e.GetEndpointAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new EndpointInfoModel {
                            Id = "endpoint1",
                            Endpoint = new EndpointModel {
                                Url = "fakeurl"
                            }
                    }));
                builder.RegisterMock(registry);
                builder.RegisterType<DataSetEntityDatabase>().AsImplementedInterfaces();
                builder.RegisterType<DataSetWriterDatabase>().AsImplementedInterfaces();
                builder.RegisterType<WriterGroupDatabase>().AsImplementedInterfaces();
                builder.RegisterType<WriterGroupRegistry>().AsImplementedInterfaces();
                builder.RegisterType<PublishServicesAdapter>().As<IPublishServices>();
            });
            return mock;
        }

        /// <summary>
        /// Mock
        /// </summary>
        public class MockConfig : IItemContainerConfig {
            public string ContainerName => "Test";
            public string DatabaseName => "Test";
        }

        public class MockRegistry : IEndpointRegistry {

            public Task<EndpointInfoModel> GetEndpointAsync(string endpointId,
                CancellationToken ct = default) {
                throw new NotImplementedException();
            }

            public Task<X509CertificateChainModel> GetEndpointCertificateAsync(
                string endpointId, CancellationToken ct = default) {
                throw new NotImplementedException();
            }

            public Task<EndpointInfoListModel> ListEndpointsAsync(string continuation,
                int? pageSize = null, CancellationToken ct = default) {
                throw new NotImplementedException();
            }

            public Task<EndpointInfoListModel> QueryEndpointsAsync(
                EndpointInfoQueryModel query,
                int? pageSize = null, CancellationToken ct = default) {
                throw new NotImplementedException();
            }

            public Task UpdateEndpointAsync(string endpointId, EndpointInfoUpdateModel model,
                CancellationToken ct = default) {
                throw new NotImplementedException();
            }
        }
    }
}
