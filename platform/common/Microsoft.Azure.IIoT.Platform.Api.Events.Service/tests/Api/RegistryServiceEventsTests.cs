// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Api {
    using Microsoft.Azure.IIoT.Platform.Discovery.Api;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Discovery.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Threading.Tasks;
    using Xunit;
    using System;
    using System.Collections.Generic;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(WebAppCollection.Name)]
    public class RegistryServiceEventsTests {

        public RegistryServiceEventsTests(WebAppFixture factory) {
            _factory = factory;
        }

        private readonly WebAppFixture _factory;

        [Fact]
        public async Task TestPublishApplicationEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var expected = new ApplicationEventModel {
                Application = new ApplicationInfoModel {
                    ApplicationType = IIoT.Platform.Core.Models.ApplicationType.Client,
                    Capabilities = new HashSet<string>{ "ag", "sadf", "" },
                }.SetAsLost()
            };
            var result = new TaskCompletionSource<ApplicationEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var expected = new ApplicationEventModel {
                Application = new ApplicationInfoModel {
                    ApplicationType = IIoT.Platform.Core.Models.ApplicationType.Client,
                    Capabilities = new HashSet<string> { "ag", "sadf", "" },
                }.SetAsLost()
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var expected = new EndpointEventModel {
                Endpoint = new EndpointInfoModel {
                    ApplicationId = "TestSigfsdfg  ff",
                }.SetAsLost()
            };
            var result = new TaskCompletionSource<EndpointEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(46340)]
        public async Task TestPublishEndpointEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var expected = new EndpointEventModel {
                Endpoint = new EndpointInfoModel {
                    ApplicationId = "TestSigfsdfg  ff",
                }.SetAsLost()
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        public async Task TestPublishDiscoveryProgressWithDiscovererIdAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var discovererId = "TestDiscoverer1";
            var expected = new DiscoveryProgressModel {
                DiscovererId = discovererId,
                Discovered = 55,
                ResultDetails = new Dictionary<string, string> { ["test"] = "test" },
                EventType = IIoT.Platform.Discovery.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<DiscoveryProgressApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                    (IIoT.Platform.Discovery.Models.DiscoveryProgressType)received.EventType);
                Assert.Equal(expected.ResultDetails, received.ResultDetails);
            }
        }

        [Fact]
        public async Task TestPublishDiscoveryProgressWithRequestIdAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

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
                EventType = IIoT.Platform.Discovery.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<DiscoveryProgressApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                    (IIoT.Platform.Discovery.Models.DiscoveryProgressType)received.EventType);
                Assert.Equal(expected.ResultDetails, received.ResultDetails);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(678)]
        public async Task TestPublishDiscoveryProgressAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDiscoveryServiceEvents>();

            var discovererId = "TestDiscoverer1";
            var expected = new DiscoveryProgressModel {
                DiscovererId = discovererId,
                Discovered = 55,
                EventType = IIoT.Platform.Discovery.Models.DiscoveryProgressType.NetworkScanFinished,
                TimeStamp = DateTime.UtcNow
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
