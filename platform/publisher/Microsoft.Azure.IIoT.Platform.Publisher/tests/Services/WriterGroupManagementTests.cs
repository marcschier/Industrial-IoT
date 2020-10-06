// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Storage.Services;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Autofac;
    using Autofac.Extras.Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using Xunit;
    using Moq;

    /// <summary>
    /// Writer group management tests
    /// </summary>
    public class WriterGroupManagementTests {

        [Fact]
        public async Task UpdateWriterGroupStateTestAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry writers = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                IWriterGroupStateUpdate service = mock.Create<WriterGroupManagement>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "Test",
                }).ConfigureAwait(false);

                // Assert
                var found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Disabled
                }).ConfigureAwait(false);
                Assert.Single(found); // Initial state is disabled

                // Act
                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Publishing).ConfigureAwait(false);
                // Assert
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Publishing
                }).ConfigureAwait(false);
                Assert.Empty(found); // No publishing if not activate

                // Act
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Publishing).ConfigureAwait(false);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Pending
                }).ConfigureAwait(false);
                Assert.Empty(found);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Publishing
                }).ConfigureAwait(false);
                Assert.Single(found); // Publishing - not pending

                // Act
                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Publishing).ConfigureAwait(false);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Publishing
                }).ConfigureAwait(false);
                Assert.Single(found);
                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Pending).ConfigureAwait(false);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Pending
                }).ConfigureAwait(false);
                Assert.Single(found);
                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Publishing).ConfigureAwait(false);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Publishing
                }).ConfigureAwait(false);
                Assert.Single(found);

                // Act
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Disabled
                }).ConfigureAwait(false);
                Assert.Single(found);

                await service.UpdateWriterGroupStateAsync(result1.WriterGroupId, WriterGroupState.Publishing).ConfigureAwait(false);

                found = await groups.QueryAllWriterGroupsAsync(new WriterGroupInfoQueryModel {
                    State = WriterGroupState.Disabled
                }).ConfigureAwait(false);
                Assert.Single(found); // No publishing if disabled
            }
        }

        [Fact]
        public async Task UpdateDataSetWriterStateTestAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry writers = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                IDataSetWriterStateUpdate service = mock.Create<WriterGroupManagement>();

                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "Test",
                }).ConfigureAwait(false);

                var result2 = await writers.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1",
                    DataSetName = "Test",
                    KeyFrameInterval = TimeSpan.FromSeconds(1),
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        Priority = 1
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                // Assert
                var found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found); // Initial state is enabled

                // Act
                var now = DateTime.UtcNow;
                await service.UpdateDataSetWriterStateAsync(result2.DataSetWriterId,
                    new PublishedDataSetSourceStateModel {
                        LastResult = new ServiceResultModel {
                            ErrorMessage = "error"
                        },
                        LastResultChange = now
                    }).ConfigureAwait(false);

                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);
                Assert.Equal("error", found.Single().DataSet.State.LastResult.ErrorMessage);
                Assert.Equal(now, found.Single().DataSet.State.LastResultChange);


                // Act
                now = DateTime.UtcNow;
                await service.UpdateDataSetWriterStateAsync(result2.DataSetWriterId,
                    new PublishedDataSetSourceStateModel {
                        LastResultChange = now
                    }).ConfigureAwait(false);

                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);
                Assert.Null(found.Single().DataSet.State.LastResult);
                Assert.Equal(now, found.Single().DataSet.State.LastResultChange);
            }
        }

        [Fact]
        public async Task UpdateDataSetVariableStateTestAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry writers = mock.Create<WriterGroupRegistry>();
                IDataSetBatchOperations batch = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                IDataSetWriterStateUpdate service = mock.Create<WriterGroupManagement>();

                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "Test",
                }).ConfigureAwait(false);

                var result2 = await writers.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1",
                    DataSetName = "Test",
                    KeyFrameInterval = TimeSpan.FromSeconds(1),
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        Priority = 1
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var result = await batch.AddVariablesToDataSetWriterAsync(result2.DataSetWriterId,
                    new DataSetAddVariableBatchRequestModel {
                        DataSetPublishingInterval = TimeSpan.FromSeconds(1),
                        Variables = new List<DataSetAddVariableRequestModel> {
                            new DataSetAddVariableRequestModel {
                                PublishedVariableNodeId = "i=2554",
                                HeartbeatInterval = TimeSpan.FromDays(1)
                            },
                            new DataSetAddVariableRequestModel {
                                PublishedVariableNodeId = "i=2555",
                                HeartbeatInterval = TimeSpan.FromDays(1)
                            },
                            new DataSetAddVariableRequestModel {
                                PublishedVariableNodeId = "i=2556",
                                HeartbeatInterval = TimeSpan.FromDays(1)
                            }
                        }
                    }).ConfigureAwait(false);

                var found = await writers.ListAllDataSetVariablesAsync(result2.DataSetWriterId).ConfigureAwait(false);
                Assert.Equal(3, found.Count);
                var v = found.First();
                var targetId = v.Id;
                var now = DateTime.UtcNow;
                Assert.NotNull(v);
                Assert.Null(v.State);

                await service.UpdateDataSetVariableStateAsync(result2.DataSetWriterId, targetId,
                    new PublishedDataSetItemStateModel {
                        ClientId = 444,
                        LastResult = new ServiceResultModel {
                            StatusCode = 55
                        },
                        ServerId = 5,
                        LastResultChange = now
                    }).ConfigureAwait(false);

                found = await writers.ListAllDataSetVariablesAsync(result2.DataSetWriterId).ConfigureAwait(false);
                v = found.FirstOrDefault(v => v.Id == targetId);
                Assert.NotNull(v);
                Assert.NotNull(v.State);
                Assert.Equal(444u, v.State.ClientId);
                Assert.Equal(5u, v.State.ServerId);
                Assert.NotNull(v.State.LastResult);
                Assert.Equal(55u, v.State.LastResult.StatusCode);
                Assert.Equal(now, v.State.LastResultChange);

                await service.UpdateDataSetVariableStateAsync(result2.DataSetWriterId, targetId,
                    new PublishedDataSetItemStateModel {
                        ClientId = 0,
                        ServerId = 0,
                        LastResultChange = now
                    }).ConfigureAwait(false);

                found = await writers.ListAllDataSetVariablesAsync(result2.DataSetWriterId).ConfigureAwait(false);
                v = found.FirstOrDefault(v => v.Id == targetId);
                Assert.NotNull(v);
                Assert.NotNull(v.State);
                Assert.Null(v.State.ClientId);
                Assert.Null(v.State.ServerId);
                Assert.Null(v.State.LastResult);
                Assert.Equal(now, v.State.LastResultChange);
            }
        }

        [Fact]
        public async Task HandleEndpointEventsTestAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry writers = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                IEndpointRegistryListener service = mock.Create<WriterGroupManagement>();

                var endpoint = new EndpointInfoModel {
                    EndpointUrl = "fakeurl",
                    Id = "endpoint1",
                    Endpoint = new EndpointModel {
                        Url = "fakeurl"
                    }
                };

                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "Test",
                }).ConfigureAwait(false);

                var result2 = await writers.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1",
                    DataSetName = "Test",
                    KeyFrameInterval = TimeSpan.FromSeconds(1),
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        Priority = 1
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                // Assert
                var found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);

                // Act
                await service.OnEndpointActivatedAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);

                // Act
                await service.OnEndpointDeactivatedAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Empty(found);

                // Act
                await service.OnEndpointDeactivatedAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Empty(found);

                // Act
                await service.OnEndpointActivatedAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);

                // Act
                await service.OnEndpointDeletedAsync(null, endpoint.Id, null).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Empty(found);

                // Act
                await service.OnEndpointDeletedAsync(null, endpoint.Id, null).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Empty(found);

                // Act
                await service.OnEndpointNewAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);

                // Act
                await service.OnEndpointDeletedAsync(null, endpoint.Id, null).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Empty(found);

                // Act
                await service.OnEndpointActivatedAsync(null, endpoint).ConfigureAwait(false);
                // Assert
                found = await writers.QueryAllDataSetWritersAsync(new DataSetWriterInfoQueryModel {
                    ExcludeDisabled = true
                }).ConfigureAwait(false);
                Assert.Single(found);
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
                builder.RegisterType<ItemContainerFactory>().As<IItemContainerFactory>();
                builder.RegisterType<DataSetEntityDatabase>().AsImplementedInterfaces();
                builder.RegisterType<DataSetWriterDatabase>().AsImplementedInterfaces();
                builder.RegisterType<WriterGroupDatabase>().AsImplementedInterfaces();
                var registry = new Mock<IEndpointRegistry>();
                registry
                    .Setup(e => e.GetEndpointAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new EndpointInfoModel {
                            EndpointUrl = "fakeurl",
                            Id = "endpoint1",
                            Endpoint = new EndpointModel {
                                Url = "fakeurl"
                            }
                    }));
                builder.RegisterMock(registry);
                builder.RegisterType<WriterGroupRegistry>().AsImplementedInterfaces();
                builder.RegisterType<WriterGroupManagement>().AsImplementedInterfaces();
            });

            return mock;
        }
    }
}

