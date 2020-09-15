// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Default {
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public class SimpleEventBusTests : IClassFixture<SimpleEventBusFixture> {
        private readonly SimpleEventBusFixture _fixture;

        public SimpleEventBusTests(SimpleEventBusFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task PublishTest1Async() {
            using (var harness = _fixture.GetHarness()) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = new Fixture().Create<Family>();

                var tcs = new TaskCompletionSource<Family>();
                var token = await bus.RegisterAsync<Family>(f => {
                    tcs.SetResult(f);
                    return Task.CompletedTask;
                });

                await bus.PublishAsync(family);

                var f = await tcs.Task;
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token);
            }
        }

        [SkippableFact]
        public async Task PublishTest2Async() {
            using (var harness = _fixture.GetHarness()) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = new Fixture().Create<Family>();
                var family2 = new Fixture().Create<Family>();

                var count = 0;
                var tcs = new TaskCompletionSource<Family>();
                var token = await bus.RegisterAsync<Family>(f => {
                    if (++count == 4) {
                        tcs.TrySetResult(f);
                    }
                    return Task.CompletedTask;
                });

                await bus.PublishAsync(family2);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(family);

                var f = await tcs.Task;
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token);
            }
        }

        [SkippableFact]
        public async Task PublishTest3Async() {
            using (var harness = _fixture.GetHarness()) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = new Fixture().Create<Family>();
                var family2 = new Fixture().Create<Family>();

                var tcs1 = new TaskCompletionSource<Family>();
                var token1 = await bus.RegisterAsync<Family>(f => {
                    tcs1.TrySetResult(f);
                    return Task.CompletedTask;
                });
                var tcs2 = new TaskCompletionSource<Family>();
                var token2 = await bus.RegisterAsync<Family>(f => {
                    tcs2.TrySetResult(f);
                    return Task.CompletedTask;
                });

                await bus.PublishAsync(family);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(family2);

                var f1 = await tcs1.Task;
                var f2 = await tcs2.Task;

                Assert.Equal(family.Id, f1.Id);
                Assert.Equal(family.LastName, f1.LastName);
                Assert.Equal(family.RegistrationDate, f1.RegistrationDate);
                Assert.Equal(family.Id, f2.Id);
                Assert.Equal(family.LastName, f2.LastName);
                Assert.Equal(family.RegistrationDate, f2.RegistrationDate);

                await bus.UnregisterAsync(token1);
                await bus.UnregisterAsync(token2);
            }
        }

        [SkippableFact]
        public async Task PublishTest4Async() {
            using (var harness = _fixture.GetHarness()) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = new Fixture().Create<Family>();

                var tcs2 = new TaskCompletionSource<Family>();
                var token2 = await bus.RegisterAsync<Family>(f => {
                    tcs2.TrySetResult(f);
                    return Task.CompletedTask;
                });
                var tcs1 = new TaskCompletionSource<Family>();
                var token1 = await bus.RegisterAsync<Family>(f => {
                    tcs1.TrySetResult(f);
                    return Task.CompletedTask;
                });
                await bus.UnregisterAsync(token2);

                await bus.PublishAsync(family);

                var f = await tcs1.Task;
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token1);
            }
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            using (var harness = _fixture.GetHarness()) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.PublishAsync<Family>(null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.RegisterAsync<Family>(null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.UnregisterAsync(null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.UnregisterAsync(string.Empty));
                await Assert.ThrowsAsync<ResourceInvalidStateException>(
                    () => bus.UnregisterAsync("bad"));

                var token = await bus.RegisterAsync<Family>(f => default);
                await bus.UnregisterAsync(token);
                await Assert.ThrowsAsync<ResourceInvalidStateException>(
                    () => bus.UnregisterAsync(token));
            }
        }
    }
}
