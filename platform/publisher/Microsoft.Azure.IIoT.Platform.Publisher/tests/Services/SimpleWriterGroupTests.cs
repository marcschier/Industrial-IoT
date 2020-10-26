// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Storage;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Publisher.Default;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.Discovery;
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Runtime;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using Autofac.Extras.Moq;
    using Moq;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Net.Sockets;
    using Xunit;
    using Opc.Ua;

    [Collection(PublishCollection.Name)]
    public class SimpleWriterGroupTests {

        public SimpleWriterGroupTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Opc.Ua.Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        [Fact]
        public async Task WriterGroupSetupPublishingTestAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry service = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                var events = mock.Create<ObservableEventFixture>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "TestGroup",
                }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var result2 = await service.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1", // See below
                    DataSetName = "TestSet",
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        PublishingInterval = TimeSpan.FromSeconds(1)
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var variable = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=2258", // server time
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Activate the group - will start the engine
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);

                // Should get a good source state
                var sevt = events.GetSourceStates(result2.DataSetWriterId).WaitForEvent();
                Assert.NotNull(sevt);
                Assert.Null(sevt.LastResult?.ErrorMessage);
                Assert.Null(sevt.LastResult?.StatusCode);

                // Should get state change for item
                var v1evt = events.GetItemStates(result2.DataSetWriterId, variable.Id)
                    .WaitForEvent(e => e.ServerId != null);
                Assert.NotNull(v1evt);
                Assert.Null(v1evt.LastResult?.ErrorMessage);
                Assert.Null(v1evt.LastResult?.StatusCode);

                // Should get messages
                var message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                Assert.NotNull(message.Data);
                Assert.NotNull(message.ContentEncoding);
                Assert.Equal(ContentMimeType.Json, message.ContentType);
                var value = message.Decode();
                Assert.False(value.IsNull());
                Assert.Equal("1", value.GetByPath("MessageId"));
                Assert.Equal("ua-data", value.GetByPath("MessageType"));
                Assert.Equal(1, value.GetByPath("Messages[0].MetaDataVersion.MajorVersion"));
                Assert.True(value.GetByPath("Messages[0].Status").IsNull());
                Assert.True(value.GetByPath("Messages[0].Payload.i=2258.Value").IsDateTime);

                message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                Assert.NotNull(message.Data);
                Assert.NotNull(message.ContentEncoding);
                Assert.Equal(ContentMimeType.Json, message.ContentType);
                value = message.Decode();
                Assert.False(value.IsNull());
                Assert.Equal("2", value.GetByPath("MessageId"));
                Assert.Equal("ua-data", value.GetByPath("MessageType"));
                Assert.Equal(1, value.GetByPath("Messages[0].MetaDataVersion.MajorVersion"));
                Assert.True(value.GetByPath("Messages[0].Status").IsNull());
                Assert.True(value.GetByPath("Messages[0].Payload.i=2258.Value").IsDateTime);

                // Deactivate - stop engine
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task WriterGroupSetupPublishingTestWithBadNodeAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry service = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                var events = mock.Create<ObservableEventFixture>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "TestGroup",
                }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var result2 = await service.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1", // See below
                    DataSetName = "TestSet",
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        PublishingInterval = TimeSpan.FromSeconds(1)
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var variable = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=88888", // bad
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Activate the group - will start the engine
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);

                // Should get a good source state
                var sevt = events.GetSourceStates(result2.DataSetWriterId).WaitForEvent();
                Assert.NotNull(sevt);
                Assert.Null(sevt.LastResult?.ErrorMessage);
                Assert.Null(sevt.LastResult?.StatusCode);

                // Should get BadNodeIdInvalid state change for item
                var v1evt = events.GetItemStates(result2.DataSetWriterId, variable.Id)
                    .WaitForEvent(e => e.LastResult?.StatusCode != null);
                Assert.NotNull(v1evt);
                Assert.Equal(StatusCodes.BadNodeIdUnknown, v1evt.LastResult?.StatusCode);
                Assert.NotNull(v1evt.LastResult?.ErrorMessage);

                // Deactivate - stop engine
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task WriterGroupSetupPublishingAddRemoveVariableAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry service = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                var events = mock.Create<ObservableEventFixture>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "TestGroup",
                }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var result2 = await service.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1", // See below
                    DataSetName = "TestSet",
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        PublishingInterval = TimeSpan.FromSeconds(1)
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var variable = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=88888", // bad
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Activate the group - will start the engine
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);

                // Should get a good source state
                var sevt = events.GetSourceStates(result2.DataSetWriterId).WaitForEvent();
                Assert.NotNull(sevt);
                Assert.Null(sevt.LastResult?.ErrorMessage);
                Assert.Null(sevt.LastResult?.StatusCode);

                // Should get BadNodeIdInvalid state change for item
                var v1evt = events.GetItemStates(result2.DataSetWriterId, variable.Id)
                    .WaitForEvent(e => e.LastResult?.StatusCode != null);
                Assert.NotNull(v1evt);
                Assert.Equal(StatusCodes.BadNodeIdUnknown, v1evt.LastResult?.StatusCode);
                Assert.NotNull(v1evt.LastResult?.ErrorMessage);

                // Add a single writer to endpoint
                variable = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=2258", // good
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Should get state change for item
                var v2evt = events.GetItemStates(result2.DataSetWriterId, variable.Id)
                    .WaitForEvent(e => e.ServerId != null);
                Assert.NotNull(v2evt);
                Assert.Null(v2evt.LastResult?.ErrorMessage);
                Assert.Null(v2evt.LastResult?.StatusCode);

                // Should get messages
                var message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                Assert.NotNull(message.Data);
                Assert.NotNull(message.ContentEncoding);
                Assert.Equal(ContentMimeType.Json, message.ContentType);

                // Deactivate - stop engine
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task WriterGroupSetupPublishingAddGoodAndBadAsync() {

            using (var mock = Setup()) {

                IDataSetWriterRegistry service = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                var events = mock.Create<ObservableEventFixture>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "TestGroup",
                }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var result2 = await service.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1", // See below
                    DataSetName = "TestSet",
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        PublishingInterval = TimeSpan.FromSeconds(1)
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var bad = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=88888", // bad
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var good = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=2258", // good
                        SamplingInterval = TimeSpan.FromSeconds(1)
                    }).ConfigureAwait(false);

                // Activate the group - will start the engine
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);

                // Should get a good source state
                var sevt = events.GetSourceStates(result2.DataSetWriterId).WaitForEvent();
                Assert.NotNull(sevt);
                Assert.Null(sevt.LastResult?.ErrorMessage);
                Assert.Null(sevt.LastResult?.StatusCode);

                // Should get BadNodeIdInvalid state change for item
                var v1evt = events.GetItemStates(result2.DataSetWriterId, bad.Id)
                    .WaitForEvent(e => e.LastResult?.StatusCode != null);
                Assert.NotNull(v1evt);
                Assert.Equal(StatusCodes.BadNodeIdUnknown, v1evt.LastResult?.StatusCode);
                Assert.NotNull(v1evt.LastResult?.ErrorMessage);

                // Should get state change for good item
                var v2evt = events.GetItemStates(result2.DataSetWriterId, good.Id)
                    .WaitForEvent(e => e.ServerId != null);
                Assert.NotNull(v2evt);
                Assert.Null(v2evt.LastResult?.ErrorMessage);
                Assert.Null(v2evt.LastResult?.StatusCode);

                // Good
                var message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message.Data);
                Assert.Equal(ContentMimeType.Json, message.ContentType);
                Assert.NotNull(message.ContentEncoding);
                Assert.NotNull(message);
                var value = message.Decode();
                Assert.False(value.IsNull());
                Assert.Equal("1", value.GetByPath("MessageId"));
                Assert.Equal("ua-data", value.GetByPath("MessageType"));
                Assert.Equal(1, value.GetByPath("Messages[0].MetaDataVersion.MajorVersion"));
                Assert.True(value.GetByPath("Messages[0].Status").IsNull());
                Assert.True(value.GetByPath("Messages[0].Payload.i=2258.Value").IsDateTime);

                // Good
                message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                Assert.NotNull(message.Data);
                Assert.NotNull(message.ContentEncoding);
                Assert.Equal(ContentMimeType.Json, message.ContentType);
                value = message.Decode();
                Assert.False(value.IsNull());
                Assert.Equal("2", value.GetByPath("MessageId"));
                Assert.Equal("ua-data", value.GetByPath("MessageType"));
                Assert.Equal(1, value.GetByPath("Messages[0].MetaDataVersion.MajorVersion"));
                Assert.True(value.GetByPath("Messages[0].Status").IsNull());
                Assert.True(value.GetByPath("Messages[0].Payload.i=2258.Value").IsDateTime);

                // Deactivate - stop engine
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, 5)]
        [InlineData(5, 100)]
        [InlineData(100, 5)]
        [InlineData(1000, 5)]
        public async Task WriterGroupSetupPublishingBatchTestAsync(int batchSize, int intervalInSec) {

            using (var mock = Setup()) {

                IDataSetWriterRegistry service = mock.Create<WriterGroupRegistry>();
                IWriterGroupRegistry groups = mock.Create<WriterGroupRegistry>();
                var events = mock.Create<ObservableEventFixture>();

                // Act
                var result1 = await groups.AddWriterGroupAsync(new WriterGroupAddRequestModel {
                    Name = "TestGroup",
                    BatchSize = batchSize,
                    // This is the key - after 5 seconds it should click
                    PublishingInterval = intervalInSec == 0 ?
                        (TimeSpan?)null : TimeSpan.FromSeconds(intervalInSec),
                }).ConfigureAwait(false);

                // Add a single writer to endpoint
                var result2 = await service.AddDataSetWriterAsync(new DataSetWriterAddRequestModel {
                    EndpointId = "endpoint1", // See below
                    DataSetName = "TestSet",
                    SubscriptionSettings = new PublishedDataSetSourceSettingsModel {
                        PublishingInterval = TimeSpan.FromMilliseconds(100)
                    },
                    WriterGroupId = result1.WriterGroupId
                }).ConfigureAwait(false);

                var variable = await service.AddDataSetVariableAsync(
                    result2.DataSetWriterId,
                    new DataSetAddVariableRequestModel {
                        PublishedVariableNodeId = "i=2258", // server time
                        SamplingInterval = TimeSpan.FromMilliseconds(100)
                    }).ConfigureAwait(false);

                // Activate the group - will start the engine
                await groups.ActivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);

                // Should get a good source state
                var sevt = events.GetSourceStates(result2.DataSetWriterId).WaitForEvent();
                Assert.NotNull(sevt);
                Assert.Null(sevt.LastResult?.ErrorMessage);
                Assert.Null(sevt.LastResult?.StatusCode);

                // Should get state change for item
                var v1evt = events.GetItemStates(result2.DataSetWriterId, variable.Id)
                    .WaitForEvent(e => e.ServerId != null);
                Assert.NotNull(v1evt);
                Assert.Null(v1evt.LastResult?.ErrorMessage);
                Assert.Null(v1evt.LastResult?.StatusCode);

                // Should get messages
                var message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                Assert.NotNull(message.Data);
                Assert.NotNull(message.ContentEncoding);
                Assert.Equal(ContentMimeType.Json, message.ContentType);
                var value = message.Decode();
                Assert.False(value.IsNull());
                Assert.True(value.IsArray);
                Assert.True(value.Count > 0);

                message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                value = message.Decode();
                Assert.False(value.IsNull());
                Assert.True(value.IsArray);
                Assert.True(value.Count > 0);

                message = events.GetMessages(null).WaitForEvent();
                Assert.NotNull(message);
                value = message.Decode();
                Assert.False(value.IsNull());
                Assert.True(value.IsArray);
                Assert.True(value.Count > 3);

                // Deactivate - stop engine
                await groups.DeactivateWriterGroupAsync(result1.WriterGroupId).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        private AutoMock Setup() {
            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterInstance(new ConfigurationBuilder().Build()).AsImplementedInterfaces();
                builder.RegisterInstance(ConsoleLogger.CreateLogger()).AsImplementedInterfaces();
                builder.RegisterType<ClientServicesConfig>().AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().As<IDatabaseServer>().SingleInstance();
                builder.RegisterType<ItemContainerFactory>().As<IItemContainerFactory>();
                builder.RegisterType<DataSetEntityDatabase>().AsImplementedInterfaces();
                builder.RegisterType<DataSetWriterDatabase>().AsImplementedInterfaces();
                builder.RegisterType<WriterGroupDatabase>().AsImplementedInterfaces();
                var registry = new Mock<IEndpointRegistry>();
                var url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer";
                registry
                    .Setup(e => e.GetEndpointAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new EndpointInfoModel {
                            Id = "endpoint1",
                            Endpoint = new EndpointModel {
                                Url = url,
                                AlternativeUrls = _hostEntry?.AddressList
                                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                    .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
                                Certificate = _server.Certificate?.RawData?.ToThumbprint()
                            }
                    }));
                builder.RegisterMock(registry);
                builder.RegisterType<DataSetWriterEventBroker>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<WriterGroupEventBroker>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<WriterGroupRegistry>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<WriterGroupRegistrySync>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<SimpleWriterGroupManager>().AsImplementedInterfaces().SingleInstance()
                    .AutoActivate(); // Create and register with broker
                builder.RegisterType<SimpleNetworkMessageSink>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<SimpleWriterGroupDataSource>().AsImplementedInterfaces();
                builder.RegisterType<NetworkMessageUadpEncoder>().AsImplementedInterfaces();
                builder.RegisterType<NetworkMessageJsonEncoder>().AsImplementedInterfaces();
                builder.RegisterType<VariantEncoderFactory>().AsImplementedInterfaces();
                builder.RegisterType<DefaultSessionManager>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<SubscriptionServices>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<ObservableEventFixture>().AsSelf().AsImplementedInterfaces().SingleInstance();
            });
            return mock;
        }

        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;
    }

}
