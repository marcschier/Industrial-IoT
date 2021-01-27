// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Events {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Events.v2.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(WebAppCollection.Name)]
    public class TwinServiceEventsTests {

        public TwinServiceEventsTests(WebAppFixture factory) {
            _factory = factory;
        }

        private readonly WebAppFixture _factory;


        [Fact]
        public async Task TestPublishTwinEventAndReceiveAsync() {

            var bus = _factory.Resolve<IEventBusPublisher>();
            var client = _factory.Resolve<ITwinServiceEvents>();

            var expected = new TwinEventModel {
                Twin = new TwinInfoModel {
                    EndpointId = "dafsdafsdafsdf",
                    Id = "tasdfdsadf"
                }
            };
            var result = new TaskCompletionSource<TwinEventApiModel>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using (await client.SubscribeTwinEventsAsync(ev => {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {

                await bus.PublishAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(5000)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted);
                var received = result.Task.Result;
                Assert.NotNull(received?.Twin);
                Assert.Equal(expected.Twin.Id, received.Twin.Id);
                Assert.Equal(expected.Twin.EndpointId, received.Twin.EndpointId);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(46340)]
        public async Task TestPublishTwinEventAndReceiveMultipleAsync(int total) {

            var bus = _factory.Resolve<IEventBusPublisher>();
            var client = _factory.Resolve<ITwinServiceEvents>();

            var expected = new TwinEventModel {
                Twin = new TwinInfoModel {
                    EndpointId = "dafsdafsdafsdf",
                    Id = "tasdfdsadf"
                }
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var counter = 0;
            await using (await client.SubscribeTwinEventsAsync(ev => {
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