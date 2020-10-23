// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Api {
    using Microsoft.Azure.IIoT.Platform.Directory.Api;
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(WebAppCollection.Name)]
    public class DirectoryServiceEventsTests {

        public DirectoryServiceEventsTests(WebAppFixture factory) {
            _factory = factory;
        }

        private readonly WebAppFixture _factory;

        [Fact]
        public async Task TestPublishPublisherEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new PublisherEventModel {
                Publisher = new PublisherModel {
                    Connected = null,
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<PublisherEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                    (IIoT.Platform.Directory.Models.TraceLogLevel)received.Publisher.LogLevel);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(678)]
        public async Task TestPublishPublisherEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new PublisherEventModel {
                Publisher = new PublisherModel {
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new DiscovererEventModel {
                Discoverer = new DiscovererModel {
                    Connected = true,
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<DiscovererEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using (await client.SubscribeDiscovererEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.Equal(true, received?.Discoverer?.Connected);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(55)]
        [InlineData(375)]
        public async Task TestPublishDiscovererEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new DiscovererEventModel {
                Discoverer = new DiscovererModel {
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new SupervisorEventModel {
                Supervisor = new SupervisorModel {
                    Connected = true,
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<SupervisorEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                    (IIoT.Platform.Directory.Models.TraceLogLevel)received.Supervisor.LogLevel);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(4634)]
        public async Task TestPublishSupervisorEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new SupervisorEventModel {
                Supervisor = new SupervisorModel {
                    LogLevel = IIoT.Platform.Directory.Models.TraceLogLevel.Verbose
                }
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        public async Task TestPublishGatewayEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new GatewayEventModel {
                EventType = IIoT.Platform.Directory.Events.v2.Models.GatewayEventType.Deleted,
                Gateway = new GatewayModel {
                    SiteId = "TestSigfsdfg  ff",
                }
            };
            var result = new TaskCompletionSource<GatewayEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                Assert.Equal(IIoT.Platform.Directory.Api.Models.GatewayEventType.Deleted,
                    received.EventType);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public async Task TestPublishGatewayEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBus>();
            var client = _factory.Resolve<IDirectoryServiceEvents>();

            var expected = new GatewayEventModel {
                Gateway = new GatewayModel {
                    SiteId = "TestSigfsdfg  ff",
                }
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
    }
}
