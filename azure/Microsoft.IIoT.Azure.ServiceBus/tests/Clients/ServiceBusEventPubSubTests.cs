// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Xunit;
    using Xunit.Categories;

    [SystemTest]
    public class ServiceBusEventPubSubTests : IClassFixture<ServiceBusEventFixture> {
        private readonly ServiceBusEventFixture _fixture;

        public ServiceBusEventPubSubTests(ServiceBusEventFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task SendTest1Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                await queue.PublishAsync(target, data).ConfigureAwait(false);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.Empty(result.Properties);
            }
        }

        [SkippableFact]
        public async Task SendTest2Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                await queue.PublishAsync(target, data, properties).ConfigureAwait(false);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableFact]
        public async Task SendTest3Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    if (++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.PublishAsync(target, fix.CreateMany<byte>().ToArray(), properties).ConfigureAwait(false);
                await queue.PublishAsync(target, fix.CreateMany<byte>().ToArray(), properties).ConfigureAwait(false);
                await queue.PublishAsync(target, fix.CreateMany<byte>().ToArray(), properties).ConfigureAwait(false);
                await queue.PublishAsync(target, data, properties).ConfigureAwait(false);
                await queue.PublishAsync(target, fix.CreateMany<byte>().ToArray(), properties).ConfigureAwait(false);
                await queue.PublishAsync(target, fix.CreateMany<byte>().ToArray(), properties).ConfigureAwait(false);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
        // [InlineData(100)]
        // [InlineData(1000)]
        // [InlineData(10000)]
        // [InlineData(100000)]
        public async Task SendTestLargeNumberOfEventsAsync(int max) {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();
                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var senders = Enumerable.Range(0, max)
                    .Select(i => queue.PublishAsync(target + "/" + rand.Next(0, 10), data, properties));
                await Task.WhenAll(senders).ConfigureAwait(false);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackTest1Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = "token";
                var actual = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                queue.Publish(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                });

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(expected, await actual.Task.ConfigureAwait(false));
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.Empty(result.Properties);
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackTest2Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = 1234;
                var actual = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
                queue.Publish(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                }, properties);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(expected, await actual.Task.ConfigureAwait(false));
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackTest3Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    if (++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                var expected = 4;
                var actual = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
                queue.Publish(target, fix.CreateMany<byte>().ToArray(), 1, (t, e) => { }, properties);
                queue.Publish(target, fix.CreateMany<byte>().ToArray(), 2, (t, e) => { }, properties);
                queue.Publish(target, fix.CreateMany<byte>().ToArray(), 3, (t, e) => { }, properties);
                queue.Publish(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                }, properties);
                queue.Publish(target, fix.CreateMany<byte>().ToArray(), 5, (t, e) => { }, properties);

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.Equal(expected, await actual.Task.ConfigureAwait(false));
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
        //  [InlineData(77)]
        //  [InlineData(888)]
        //  [InlineData(8435)]
        //  [InlineData(35234)]
        public async Task SendWithCallbackLargeNumberOfEventsAsync(int max) {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<Dictionary<string, string>>().ToEventProperties();
                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var hashSet = new HashSet<int>(max);
                Enumerable.Range(0, max)
                    .ToList()
                    .ForEach(i => queue.Publish(target + "/" + rand.Next(0, 100), data, i,
                        (t, e) => hashSet.Add(t), properties));

                var result = await tcs.Task.With2MinuteTimeout().ConfigureAwait(false);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.IsSameAs(properties));
            }
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventPublisherClient();
                Skip.If(queue == null);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.PublishAsync(target, (byte[])null)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.PublishAsync(null, new byte[4])).ConfigureAwait(false);
                Assert.Throws<ArgumentNullException>(
                    () => queue.Publish(target, null, "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Publish(null, new byte[4], "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Publish<string>(target, new byte[4], null, (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Publish(target, new byte[4], "test", null));
            }
        }
    }
}
