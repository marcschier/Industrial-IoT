// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Default {
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Xunit;

    public class SimpleEventQueueClientTests : IClassFixture<SimpleEventQueueFixture> {
        private readonly SimpleEventQueueFixture _fixture;

        public SimpleEventQueueClientTests(SimpleEventQueueFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task SendTest1Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                await queue.SendAsync(target, data);

                var result = await tcs.Task;
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
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                await queue.SendAsync(target, data, properties);

                var result = await tcs.Task;
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task SendTest3Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendAsync(target, fix.CreateMany<byte>().ToArray(), properties);
                await queue.SendAsync(target, fix.CreateMany<byte>().ToArray(), properties);
                await queue.SendAsync(target, fix.CreateMany<byte>().ToArray(), properties);
                await queue.SendAsync(target, data, properties);
                await queue.SendAsync(target, fix.CreateMany<byte>().ToArray(), properties);
                await queue.SendAsync(target, fix.CreateMany<byte>().ToArray(), properties);

                var result = await tcs.Task;
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task SendTestLargeNumberOfEventsAsync() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();
                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == 1000) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var senders = Enumerable.Range(0, 1000)
                    .Select(i => queue.SendAsync(target + "/" + rand.Next(0, 10), data, properties));
                await Task.WhenAll(senders);

                var result = await tcs.Task;
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackTest1Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = "token";
                var actual = new TaskCompletionSource<string>();
                queue.Send(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                });

                var result = await tcs.Task;
                Assert.Equal(expected, await actual.Task);
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
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = 1234;
                var actual = new TaskCompletionSource<int?>();
                queue.Send(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                }, properties);

                var result = await tcs.Task;
                Assert.Equal(expected, await actual.Task);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackTest3Async() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                var expected = 4;
                var actual = new TaskCompletionSource<int?>();
                queue.Send(target, fix.CreateMany<byte>().ToArray(), 1, (t, e) => { }, properties);
                queue.Send(target, fix.CreateMany<byte>().ToArray(), 2, (t, e) => { }, properties);
                queue.Send(target, fix.CreateMany<byte>().ToArray(), 3, (t, e) => { }, properties);
                queue.Send(target, data, expected, (t, e) => {
                    if (e != null) {
                        actual.TrySetException(e);
                    }
                    else {
                        actual.TrySetResult(t);
                    }
                }, properties);
                queue.Send(target, fix.CreateMany<byte>().ToArray(), 5, (t, e) => { }, properties);

                var result = await tcs.Task;
                Assert.Equal(expected, await actual.Task);
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task SendWithCallbackLargeNumberOfEventsAsync() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var properties = fix.Create<IDictionary<string, string>>();
                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == 10000) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var hashSet = new HashSet<int>(10000);
                Enumerable.Range(0, 10000)
                    .ToList()
                    .ForEach(i => queue.Send(target + "/" + rand.Next(0, 100), data, i,
                        (t, e) => hashSet.Add(t), properties));

                var result = await tcs.Task;
                Assert.True(data.SequenceEqualsSafe(result.Data));
                Assert.Null(result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.True(result.Properties.DictionaryEqualsSafe(properties));
            }
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            var fix = new Fixture();
            var target = fix.Create<string>();
            using (var harness = _fixture.GetHarness(target)) {
                var queue = harness.GetEventQueueClient();
                Skip.If(queue == null);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendAsync(target, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendAsync(null, new byte[4]));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Send(target, null, "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Send(null, new byte[4], "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Send<string>(target, new byte[4], null, (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.Send(target, new byte[4], "test", null));
            }
        }
    }
}
