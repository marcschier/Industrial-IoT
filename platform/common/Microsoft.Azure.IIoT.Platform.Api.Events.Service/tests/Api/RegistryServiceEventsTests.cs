// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Api {
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Threading.Tasks;
    using Xunit;
    using System;
    using System.Collections.Generic;

    [Collection(WebAppCollection.Name)]
    public class RegistryServiceEventsTests {

        public RegistryServiceEventsTests(WebAppFixture factory) {
            _factory = factory;
        }

        private readonly WebAppFixture _factory;

        [Fact]
        public async Task TestPublishPublisherEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new PublisherEventModel {
                Publisher = new PublisherModel {
                    Connected = null,
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<PublisherEventApiModel>();
            await using (await client.SubscribePublisherEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.Null(received?.Publisher?.Connected);
                Assert.Equal(expected.Publisher.LogLevel,
                    (IIoT.Platform.Registry.Models.TraceLogLevel)received.Publisher.LogLevel);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(678)]
        public async Task TestPublishPublisherEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new PublisherEventModel {
                Publisher = new PublisherModel {
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribePublisherEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }

        [Fact]
        public async Task TestPublishDiscovererEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new DiscovererEventModel {
                Discoverer = new DiscovererModel {
                    Connected = true,
                    Discovery = IIoT.Platform.Registry.Models.DiscoveryMode.Local,
                    DiscoveryConfig = new DiscoveryConfigModel {
                        IdleTimeBetweenScans = TimeSpan.FromSeconds(5)
                    },
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<DiscovererEventApiModel>();
            await using (await client.SubscribeDiscovererEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Discoverer?.DiscoveryConfig);
                Assert.Equal(true, received?.Discoverer?.Connected);
                Assert.Equal(TimeSpan.FromSeconds(5),
                    expected.Discoverer.DiscoveryConfig.IdleTimeBetweenScans);
                Assert.Equal(expected.Discoverer.LogLevel,
                    (IIoT.Platform.Registry.Models.TraceLogLevel)received.Discoverer.LogLevel);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(55)]
        [InlineData(375)]
        public async Task TestPublishDiscovererEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new DiscovererEventModel {
                Discoverer = new DiscovererModel {
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeDiscovererEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }


        [Fact]
        public async Task TestPublishSupervisorEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new SupervisorEventModel {
                Supervisor = new SupervisorModel {
                    Connected = true,
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<SupervisorEventApiModel>();
            await using (await client.SubscribeSupervisorEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Supervisor);
                Assert.Equal(true, received?.Supervisor?.Connected);
                Assert.Equal(expected.Supervisor.LogLevel,
                    (IIoT.Platform.Registry.Models.TraceLogLevel)received.Supervisor.LogLevel);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(4634)]
        public async Task TestPublishSupervisorEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new SupervisorEventModel {
                Supervisor = new SupervisorModel {
                    LogLevel = IIoT.Platform.Registry.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeSupervisorEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }


        [Fact]
        public async Task TestPublishApplicationEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new ApplicationEventModel {
                Application = new ApplicationInfoModel {
                    SiteId = "TestSigfsdfg  ff",
                    ApplicationType = IIoT.Platform.Core.Models.ApplicationType.Client,
                    NotSeenSince = DateTime.UtcNow,
                    Capabilities = new HashSet<string>{ "ag", "sadf", "" },
                }
            };
            var result = new TaskCompletionSource<ApplicationEventApiModel>();
            await using (await client.SubscribeApplicationEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Application);
                Assert.Equal(expected.Application.NotSeenSince, received.Application.NotSeenSince);
                Assert.Equal(expected.Application.SiteId, received.Application.SiteId);
                Assert.True(expected.Application.Capabilities
                    .SequenceEqualsSafe(received.Application.Capabilities));
                Assert.Equal(expected.Application.ApplicationType,
                    (IIoT.Platform.Core.Models.ApplicationType)received.Application.ApplicationType);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(4634)]
        public async Task TestPublishApplicationEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new ApplicationEventModel {
                Application = new ApplicationInfoModel {
                    SiteId = "TestSigfsdfg  ff",
                    ApplicationType = IIoT.Platform.Core.Models.ApplicationType.Client,
                    NotSeenSince = DateTime.UtcNow,
                    Capabilities = new HashSet<string> { "ag", "sadf", "" },
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeApplicationEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }

        [Fact]
        public async Task TestPublishEndpointEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new EndpointEventModel {
                Endpoint = new EndpointInfoModel {
                    ApplicationId = "TestSigfsdfg  ff",
                    ActivationState = IIoT.Platform.Registry.Models.EntityActivationState.ActivatedAndConnected,
                    NotSeenSince = DateTime.UtcNow
                }
            };
            var result = new TaskCompletionSource<EndpointEventApiModel>();
            await using (await client.SubscribeEndpointEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Endpoint);
                Assert.Equal(expected.Endpoint.NotSeenSince, received.Endpoint.NotSeenSince);
                Assert.Equal(expected.Endpoint.ApplicationId, received.Endpoint.ApplicationId);
                Assert.Equal(expected.Endpoint.ActivationState,
                    (IIoT.Platform.Registry.Models.EntityActivationState)received.Endpoint.ActivationState);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(46340)]
        public async Task TestPublishEndpointEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new EndpointEventModel {
                Endpoint = new EndpointInfoModel {
                    ApplicationId = "TestSigfsdfg  ff",
                    ActivationState = IIoT.Platform.Registry.Models.EntityActivationState.ActivatedAndConnected,
                    NotSeenSince = DateTime.UtcNow
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeEndpointEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }

        [Fact]
        public async Task TestPublishGatewayEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new GatewayEventModel {
                EventType = IIoT.Platform.Registry.Events.v2.Models.GatewayEventType.Deleted,
                Gateway = new GatewayModel {
                    SiteId = "TestSigfsdfg  ff",
                }
            };
            var result = new TaskCompletionSource<GatewayEventApiModel>();
            await using (await client.SubscribeGatewayEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Gateway);
                Assert.Equal(expected.Gateway.SiteId, received.Gateway.SiteId);
                Assert.Equal(IIoT.Platform.Registry.Api.Models.GatewayEventType.Deleted,
                    received.EventType);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public async Task TestPublishGatewayEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var expected = new GatewayEventModel {
                Gateway = new GatewayModel {
                    SiteId = "TestSigfsdfg  ff",
                }
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeGatewayEventsAsync(ev => {
                counter++;
                if (counter == total) {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }

        [Fact]
        public async Task TestPublishDiscoveryProgressWithDiscovererIdAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var discovererId = "TestDiscoverer1";
            var expected = new DiscoveryProgressModel {
                DiscovererId = discovererId,
                Discovered = 55,
                ResultDetails = new Dictionary<string, string> { ["test"] = "test" },
                EventType = IIoT.Platform.Registry.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<DiscoveryProgressApiModel>();
            await using (await client.SubscribeDiscoveryProgressByDiscovererIdAsync(
                discovererId, ev => {
                    result.SetResult(ev);
                    return Task.CompletedTask;
                }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received);
                Assert.Equal(expected.DiscovererId, received.DiscovererId);
                Assert.Equal(expected.TimeStamp, received.TimeStamp);
                Assert.Equal(expected.Discovered, received.Discovered);
                Assert.Equal(expected.EventType,
                    (IIoT.Platform.Registry.Models.DiscoveryProgressType)received.EventType);
                Assert.Equal(expected.ResultDetails, received.ResultDetails);
            }
        }

        [Fact]
        public async Task TestPublishDiscoveryProgressWithRequestIdAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var requestId = "TestDiscoverer1";
            var expected = new DiscoveryProgressModel {
                Request = new DiscoveryRequestModel {
                    Id = requestId,
                    Configuration = new DiscoveryConfigModel {
                        AddressRangesToScan = "ttttttt"
                    }
                },
                DiscovererId = "testetests",
                Discovered = 55,
                ResultDetails = new Dictionary<string, string> { ["test"] = "test" },
                EventType = IIoT.Platform.Registry.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<DiscoveryProgressApiModel>();
            await using (await client.SubscribeDiscoveryProgressByRequestIdAsync(
                requestId, ev => {
                    result.SetResult(ev);
                    return Task.CompletedTask;
                }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received);
                Assert.Equal(expected.DiscovererId, received.DiscovererId);
                Assert.Equal(expected.TimeStamp, received.TimeStamp);
                Assert.Equal(expected.Discovered, received.Discovered);
                Assert.Equal(expected.EventType,
                    (IIoT.Platform.Registry.Models.DiscoveryProgressType)received.EventType);
                Assert.Equal(expected.ResultDetails, received.ResultDetails);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(678)]
        public async Task TestPublishDiscoveryProgressAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IRegistryServiceEvents>();

            var discovererId = "TestDiscoverer1";
            var expected = new DiscoveryProgressModel {
                DiscovererId = discovererId,
                Discovered = 55,
                EventType = IIoT.Platform.Registry.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<bool>();
            var counter = 0;
            await using (await client.SubscribeDiscoveryProgressByDiscovererIdAsync(
                discovererId, ev => {
                    counter++;
                    if (counter == total) {
                        result.SetResult(true);
                    }
                    return Task.CompletedTask;
                }).ConfigureAwait(false)) {

                for (var i = 0; i < total; i++) {
                    await bus.PublishAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted);
            }
        }

    }
}
