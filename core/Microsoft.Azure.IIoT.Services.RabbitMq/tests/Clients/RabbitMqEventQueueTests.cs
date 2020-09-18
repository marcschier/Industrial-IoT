// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Microsoft.Azure.IIoT.Hub;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Xunit;

    public class RabbitMqEventQueueTests : IClassFixture<RabbitMqEventQueueFixture> {
        private readonly RabbitMqEventQueueFixture _fixture;

        public RabbitMqEventQueueTests(RabbitMqEventQueueFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task SendDeviceEventTest1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test1"){
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), data, contentType, "Test1", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test1", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test1", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendDeviceEventTest2Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test1" && ++count == 5) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TEst1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TeST1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TeSt1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TesT1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), data, contentType, "test1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TESt1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null), fix.CreateMany<byte>().ToArray(), contentType, "TEST1", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("test1", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test1", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendDeviceEventTestBatch1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test1" && ++count == 16) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null),
                    Enumerable.Range(0, 10).Select(i => fix.CreateMany<byte>().ToArray()), contentType,
                    "TEst1", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null),
                    Enumerable.Range(1, 5).Select(i => fix.CreateMany<byte>().ToArray()), contentType,
                    "bbbb", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null),
                    Enumerable.Range(0, 10).Select(i => data), contentType,
                    "TEst1", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("TEst1", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test1", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
       // [InlineData(50)]
       // [InlineData(100)]
       // [InlineData(1000)]
        public async Task SendDeviceEventTestBatch2Async(int max) {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, null),
                    Enumerable.Range(0, max).Select(i => data), contentType,
                    "Test3", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test3", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test3", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendDeviceEventWithCallbackTest1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = "ttttttttttttt";
                var actual = new TaskCompletionSource<string>();
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, contentType,
                    "Test3", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test3", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test3", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendDeviceEventWithCallbackTest2Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = 1234;
                var actual = new TaskCompletionSource<int?>();
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, contentType,
                    "Test3", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test3", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test3", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendDeviceEventWithCallbackTest3Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test3" && ++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                var expected = 4;
                var actual = new TaskCompletionSource<int?>();
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, fix.Create<string>(),
                    "Test3", fix.Create<string>(), 1, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, fix.Create<string>(),
                    "Test3", fix.Create<string>(), 2, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, fix.Create<string>(),
                    "Test3", fix.Create<string>(), 3, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, contentType,
                    "Test3", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, fix.Create<string>(),
                    "Test3", fix.Create<string>(), 5, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, null), data, fix.Create<string>(),
                    "Test3", fix.Create<string>(), 6, (t, e) => { });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test3", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test3", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
        // [InlineData(50)]
        // [InlineData(100)]
        // [InlineData(1000)]
        // [InlineData(10000)]
        public async Task SendDeviceEventWithCallbackLargeNumberOfEventsAsync(int max) {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var hashSet = new HashSet<int>(max);
                Enumerable.Range(0, max)
                    .ToList()
                    .ForEach(i => queue.SendEvent(HubResource.Format(hub, deviceId, null), data, contentType,
                        "TesT3", contentEncoding, i, (t, e) => hashSet.Add(t)));

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Null(result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("TesT3", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test3", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventTest1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), data, contentType, "test2", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventTest2Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test2" && ++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), fix.CreateMany<byte>().ToArray(), contentType, "Test2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), fix.CreateMany<byte>().ToArray(), contentType, "Test2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), fix.CreateMany<byte>().ToArray(), contentType, "Test2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), data, contentType, "test2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), fix.CreateMany<byte>().ToArray(), contentType, "Test2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId), fix.CreateMany<byte>().ToArray(), contentType, "aaaaa", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventTestBatch1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test2" && ++count == 19) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId),
                    Enumerable.Range(0, 10).Select(i => fix.CreateMany<byte>().ToArray()), contentType,
                    "TEst2", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId),
                    Enumerable.Range(1, 5).Select(i => fix.CreateMany<byte>().ToArray()), contentType,
                    "bbbb", contentEncoding);
                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId),
                    Enumerable.Range(0, 10).Select(i => data), contentType,
                    "TEst2", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("TEst2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
        // [InlineData(50)]
        // [InlineData(100)]
        // [InlineData(1000)]
        public async Task SendModuleEventTestBatch2Async(int max) {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                await queue.SendEventAsync(HubResource.Format(hub, deviceId, moduleId),
                    Enumerable.Range(0, max).Select(i => data), contentType,
                    "Test2", contentEncoding);

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventWithCallbackTest1Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = "ttttttttttttt";
                var actual = new TaskCompletionSource<string>();
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, contentType,
                    "Test2", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventWithCallbackTest2Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    tcs.TrySetResult(a);
                };

                var expected = 1234;
                var actual = new TaskCompletionSource<int?>();
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, contentType,
                    "Test2", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task SendModuleEventWithCallbackTest3Async() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (a.HandlerSchema == "Test2" && ++count == 4) {
                        tcs.TrySetResult(a);
                    }
                };

                var expected = 4;
                var actual = new TaskCompletionSource<int?>();
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, fix.Create<string>(),
                    "Test2", fix.Create<string>(), 1, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, fix.Create<string>(),
                    "Test2", fix.Create<string>(), 2, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, fix.Create<string>(),
                    "Test2", fix.Create<string>(), 3, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, contentType,
                    "Test2", contentEncoding, expected, (t, e) => {
                        if (e != null) {
                            actual.TrySetException(e);
                        }
                        else {
                            actual.TrySetResult(t);
                        }
                    });
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, fix.Create<string>(),
                    "Test2", fix.Create<string>(), 5, (t, e) => { });
                queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, fix.Create<string>(),
                    "Test2", fix.Create<string>(), 6, (t, e) => { });

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(expected, await actual.Task);
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("Test2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableTheory]
        [InlineData(10)]
        // [InlineData(50)]
        // [InlineData(100)]
        // [InlineData(1000)]
        // [InlineData(10000)]
        public async Task SendModuleEventWithCallbackLargeNumberOfEventsAsync(int max) {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                var data = fix.CreateMany<byte>().ToArray();
                var deviceId = fix.Create<string>();
                var moduleId = fix.Create<string>();
                var contentEncoding = fix.Create<string>();
                var contentType = fix.Create<string>();

                var count = 0;
                var tcs = new TaskCompletionSource<TelemetryEventArgs>();
                harness.OnEvent += (_, a) => {
                    if (++count == max) {
                        tcs.TrySetResult(a);
                    }
                };

                var rand = new Random();
                var hashSet = new HashSet<int>(max);
                Enumerable.Range(0, max)
                    .ToList()
                    .ForEach(i => queue.SendEvent(HubResource.Format(hub, deviceId, moduleId), data, contentType,
                        "TesT2", contentEncoding, i, (t, e) => hashSet.Add(t)));

                var result = await tcs.Task.With1MinuteTimeout();
                Assert.Equal(deviceId, result.DeviceId);
                Assert.Equal(moduleId, result.ModuleId);
                Assert.NotEmpty(result.Properties);
                Assert.Equal(contentType, result.Properties[EventProperties.ContentType]);
                Assert.Equal(contentEncoding, result.Properties[EventProperties.ContentEncoding]);
                Assert.Equal("TesT2", result.Properties[EventProperties.EventSchema]);
                Assert.Equal("Test2", result.HandlerSchema);
                Assert.True(data.SequenceEqualsSafe(result.Data));
            }
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            var fix = new Fixture();
            var hub = fix.Create<string>();
            using (var harness = _fixture.GetHarness(hub)) {
                var queue = harness.GetEventClient();
                Skip.If(queue == null);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendEventAsync(hub, (byte[])null, null, null, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendEventAsync(hub, (IEnumerable<byte[]>)null, null, null, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendEventAsync(null, new byte[4], null, null, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => queue.SendEventAsync(null, new byte[4].YieldReturn(), null, null, null));
                Assert.Throws<ArgumentNullException>(
                    () => queue.SendEvent(null, new byte[4], null, null, null, "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.SendEvent(hub, null, null, null, null, "test", (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.SendEvent<string>(hub, new byte[4], null, null, null, null, (t, e) => { }));
                Assert.Throws<ArgumentNullException>(
                    () => queue.SendEvent(hub, new byte[4], null, null, null, "test", null));
            }
        }
    }
}
