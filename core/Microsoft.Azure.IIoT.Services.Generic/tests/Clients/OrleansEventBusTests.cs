// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    [Collection(OrleansCollection.Name)]
    public class OrleansEventBusTests : IClassFixture<OrleansEventBusFixture> {
        private readonly OrleansEventBusFixture _fixture;

        public OrleansEventBusTests(OrleansEventBusFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task PublishTest1Async() {
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix)) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = fix.Create<Family>();

                var tcs = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token = await bus.RegisterAsync<Family>(f => {
                    tcs.SetResult(f);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                await bus.PublishAsync(family).ConfigureAwait(false);

                var f = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task PublishTest2Async() {
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix)) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = fix.Create<Family>();
                var family2 = fix.Create<Family>();

                var count = 0;
                var tcs = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token = await bus.RegisterAsync<Family>(f => {
                    if (++count == 4) {
                        tcs.TrySetResult(f);
                    }
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(family).ConfigureAwait(false);

                var f = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task PublishTest3Async() {
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix)) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = fix.Create<Family>();
                var family2 = fix.Create<Family>();

                var tcs1 = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token1 = await bus.RegisterAsync<Family>(f => {
                    tcs1.TrySetResult(f);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
                var tcs2 = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token2 = await bus.RegisterAsync<Family>(f => {
                    tcs2.TrySetResult(f);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                await bus.PublishAsync(family).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);

                var f1 = await tcs1.Task.With2MinuteTimeout().ConfigureAwait(false);
                var f2 = await tcs2.Task.With2MinuteTimeout().ConfigureAwait(false);

                Assert.Equal(family.Id, f1.Id);
                Assert.Equal(family.LastName, f1.LastName);
                Assert.Equal(family.RegistrationDate, f1.RegistrationDate);
                Assert.Equal(family.Id, f2.Id);
                Assert.Equal(family.LastName, f2.LastName);
                Assert.Equal(family.RegistrationDate, f2.RegistrationDate);

                await bus.UnregisterAsync(token1).ConfigureAwait(false);
                await bus.UnregisterAsync(token2).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task PublishTest4Async() {
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix)) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family = fix.Create<Family>();

                var tcs2 = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token2 = await bus.RegisterAsync<Family>(f => {
                    tcs2.TrySetResult(f);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
                var tcs1 = new TaskCompletionSource<Family>(TaskCreationOptions.RunContinuationsAsynchronously);
                var token1 = await bus.RegisterAsync<Family>(f => {
                    tcs1.TrySetResult(f);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
                await bus.UnregisterAsync(token2).ConfigureAwait(false);

                await bus.PublishAsync(family).ConfigureAwait(false);

                var f = await tcs1.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(family.Id, f.Id);
                Assert.Equal(family.LastName, f.LastName);
                Assert.Equal(family.RegistrationDate, f.RegistrationDate);

                await bus.UnregisterAsync(token1).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix)) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.PublishAsync<Family>(null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.RegisterAsync<Family>(null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.UnregisterAsync(null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => bus.UnregisterAsync(string.Empty)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceInvalidStateException>(
                    () => bus.UnregisterAsync("bad")).ConfigureAwait(false);

                var token = await bus.RegisterAsync<Family>(f => default).ConfigureAwait(false);
                await bus.UnregisterAsync(token).ConfigureAwait(false);
                await Assert.ThrowsAsync<ResourceInvalidStateException>(
                    () => bus.UnregisterAsync(token)).ConfigureAwait(false);
            }
        }
    }
}
