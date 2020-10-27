// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.Tunnel.Services {
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Rpc.Default;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Messaging;
    using AutoFixture;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using System.Net.Http;
    using AutoFixture.AutoMoq;
    using Microsoft.Azure.IIoT.Diagnostics;

    public class HttpTunnelTests {

        [Fact]
        public async Task TestGetWebAsync() {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Setup
            var logger = ConsoleLogger.CreateLogger();
            var eventBridge = new EventBridge();
            using var factory = new HttpTunnelEventClientFactory(eventBridge, _serializer, null,
                fixture.Create<IIdentity>(), logger);
            using var clientFactory = new HttpClientFactory(factory, logger);
            var client = clientFactory.CreateClient("msft");

            var adapter = new MethodHandlerAdapter(factory.YieldReturn());
            var chunkServer = new TestChunkServer(_serializer, 100, (target, method, buffer, type) => {
                Assert.Equal(MethodNames.Response, method);
                return adapter.InvokeAsync(target, method, buffer, type).Result;
            });

            using var serverFactory = new HttpClientFactory(logger);
            var server = new HttpTunnelServer(
                new Http.Clients.HttpClient(serverFactory, logger),
                chunkServer.CreateClient(), _serializer, logger);
            eventBridge.Handler = server;

            // Act

            using var result = await client.GetAsync(new Uri("https://www.microsoft.com")).ConfigureAwait(false);

            // Assert

            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.Headers);
            var payload = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.NotNull(payload);
            Assert.NotNull(result.Headers);
            Assert.True(result.Headers.Any());
            Assert.Contains("<!DOCTYPE html>", payload, StringComparison.InvariantCulture);
        }


        [Fact]
        public async Task TestGetAsync() {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Setup
            var logger = ConsoleLogger.CreateLogger();
            var eventBridge = new EventBridge();
            using var factory = new HttpTunnelEventClientFactory(eventBridge, _serializer, null,
                fixture.Create<IIdentity>(), logger);
            using var clientFactory = new HttpClientFactory(factory, logger);
            var client = clientFactory.CreateClient("msft");

            var adapter = new MethodHandlerAdapter(factory.YieldReturn());
            var chunkServer = new TestChunkServer(_serializer, 1000, (target, method, buffer, type) => {
                Assert.Equal(MethodNames.Response, method);
                return adapter.InvokeAsync(target, method, buffer, type).Result;
            });

            var rand = new Random();
            var fix = new Fixture();
            var responseBuffer = new byte[10000];
            rand.NextBytes(responseBuffer);
            var response = Mock.Of<IHttpResponse>(r =>
                r.Content == responseBuffer &&
                r.StatusCode == System.Net.HttpStatusCode.OK);
            var httpclientMock = Mock.Of<IHttpClient>();
            Mock.Get(httpclientMock)
                .Setup(m => m.GetAsync(It.IsAny<IHttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            var server = new HttpTunnelServer(httpclientMock,
                chunkServer.CreateClient(), _serializer, logger);
            eventBridge.Handler = server;

            // Act

            using var result = await client.GetAsync(new Uri("https://test/test/test?test=test")).ConfigureAwait(false);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.Headers);
            var payload = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Assert.Equal(response.Content.Length, payload.Length);
            Assert.Equal(responseBuffer, payload);
            Assert.NotNull(result.Headers);
            Assert.Empty(result.Headers);
        }


        [Theory]
        [InlineData(5 * 1024 * 1024)]
        [InlineData(1000 * 1024)]
        [InlineData(100000)]
        [InlineData(20)]
        [InlineData(13)]
        [InlineData(1)]
        [InlineData(0)]
        public async Task TestPostAsync(int requestSize) {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Setup
            var logger = ConsoleLogger.CreateLogger();
            var eventBridge = new EventBridge();
            using var factory = new HttpTunnelEventClientFactory(eventBridge, _serializer, null,
                fixture.Create<IIdentity>(), logger);
            using var clientFactory = new HttpClientFactory(factory, logger);
            var client = clientFactory.CreateClient("msft");

            var adapter = new MethodHandlerAdapter(factory.YieldReturn());
            var chunkServer = new TestChunkServer(_serializer, 100000, (target, method, buffer, type) => {
                Assert.Equal(MethodNames.Response, method);
                return adapter.InvokeAsync(target, method, buffer, type).Result;
            });

            var uri = new Uri("https://test/test/test?test=test");
            var rand = new Random();
            var fix = new Fixture();
            var requestBuffer = new byte[requestSize];
            rand.NextBytes(requestBuffer);
            var responseBuffer = new byte[10000];
            rand.NextBytes(responseBuffer);
            var request = Mock.Of<IHttpRequest>(r =>
                r.Uri == uri);
            var response = Mock.Of<IHttpResponse>(r =>
                r.Content == responseBuffer &&
                r.StatusCode == System.Net.HttpStatusCode.OK);
            var httpclientMock = Mock.Of<IHttpClient>();
            Mock.Get(httpclientMock)
                .Setup(m => m.PostAsync(It.IsAny<IHttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            Mock.Get(httpclientMock)
                .Setup(m => m.NewRequest(It.Is<Uri>(u => u == uri), It.IsAny<string>()))
                .Returns(request);

            var server = new HttpTunnelServer(httpclientMock, chunkServer.CreateClient(), _serializer, logger);
            eventBridge.Handler = server;

            // Act

            using var content = new ByteArrayContent(requestBuffer);
            using var result = await client.PostAsync(uri, content).ConfigureAwait(false);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.Headers);
            var payload = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Assert.Equal(response.Content.Length, payload.Length);
            Assert.Equal(responseBuffer, payload);
            Assert.NotNull(result.Headers);
            Assert.Empty(result.Headers);
        }

        [Theory]
        [InlineData(5 * 1024 * 1024)]
        [InlineData(1000 * 1024)]
        [InlineData(100000)]
        [InlineData(20)]
        [InlineData(13)]
        [InlineData(1)]
        [InlineData(0)]
        public async Task TestPutAsync(int requestSize) {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Setup
            var logger = ConsoleLogger.CreateLogger();
            var eventBridge = new EventBridge();
            using var factory = new HttpTunnelEventClientFactory(eventBridge, _serializer, null,
                fixture.Create<IIdentity>(), logger);
            using var clientFactory = new HttpClientFactory(factory, logger);
            var client = clientFactory.CreateClient("msft");

            var adapter = new MethodHandlerAdapter(factory.YieldReturn());
            var chunkServer = new TestChunkServer(_serializer, 128 * 1024, (target, method, buffer, type) => {
                Assert.Equal(MethodNames.Response, method);
                return adapter.InvokeAsync(target, method, buffer, type).Result;
            });

            var uri = new Uri("https://test/test/test?test=test");
            var rand = new Random();
            var fix = new Fixture();
            var requestBuffer = new byte[requestSize];
            rand.NextBytes(requestBuffer);
            var request = Mock.Of<IHttpRequest>(r =>
                r.Uri == uri);
            var response = Mock.Of<IHttpResponse>(r =>
                r.Content == null &&
                r.StatusCode == System.Net.HttpStatusCode.OK);
            var httpclientMock = Mock.Of<IHttpClient>();
            Mock.Get(httpclientMock)
                .Setup(m => m.PutAsync(It.IsAny<IHttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            Mock.Get(httpclientMock)
                .Setup(m => m.NewRequest(It.Is<Uri>(u => u == uri), It.IsAny<string>()))
                .Returns(request);

            var server = new HttpTunnelServer(httpclientMock, chunkServer.CreateClient(), _serializer, logger);
            eventBridge.Handler = server;

            // Act

            using var content = new ByteArrayContent(requestBuffer);
            using var result = await client.PutAsync(uri, content).ConfigureAwait(false);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.Headers);
            Assert.Equal(0, result.Content.Headers.ContentLength);
            Assert.NotNull(result.Headers);
            Assert.Empty(result.Headers);
        }

        [Fact]
        public async Task TestDeleteAsync() {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Setup
            var logger = ConsoleLogger.CreateLogger();
            var eventBridge = new EventBridge();
            using var factory = new HttpTunnelEventClientFactory(eventBridge, _serializer, null,
                fixture.Create<IIdentity>(), logger);
            using var clientFactory = new HttpClientFactory(factory, logger);
            var client = clientFactory.CreateClient("msft");

            var adapter = new MethodHandlerAdapter(factory.YieldReturn());
            var chunkServer = new TestChunkServer(_serializer, 128 * 1024, (target, method, buffer, type) => {
                Assert.Equal(MethodNames.Response, method);
                return adapter.InvokeAsync(target, method, buffer, type).Result;
            });

            var uri = new Uri("https://test/test/test?test=test");
            var rand = new Random();
            var fix = new Fixture();
            var request = Mock.Of<IHttpRequest>(r =>
                r.Uri == uri);
            var response = Mock.Of<IHttpResponse>(r =>
                r.Content == null &&
                r.StatusCode == System.Net.HttpStatusCode.OK);
            var httpclientMock = Mock.Of<IHttpClient>();
            Mock.Get(httpclientMock)
                .Setup(m => m.DeleteAsync(It.IsAny<IHttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            Mock.Get(httpclientMock)
                .Setup(m => m.NewRequest(It.Is<Uri>(u => u == uri), It.IsAny<string>()))
                .Returns(request);

            var server = new HttpTunnelServer(httpclientMock, chunkServer.CreateClient(), _serializer, logger);
            eventBridge.Handler = server;

            // Act

            using var result = await client.DeleteAsync(uri).ConfigureAwait(false);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.Headers);
            Assert.Equal(0, result.Content.Headers.ContentLength);
            Assert.NotNull(result.Headers);
            Assert.Empty(result.Headers);
        }

        private class EventBridge : IEventClient {

            /// <summary>
            /// Handler
            /// </summary>
            public ITelemetryHandler Handler { get; set; }

            /// <inheritdoc/>
            public Task SendEventAsync(string target, byte[] data, string contentType,
                string eventSchema, string contentEncoding, CancellationToken ct) {
                return Handler.HandleAsync(target, data, new Dictionary<string, string> {
                    ["content-type"] = contentType
                }, () => Task.CompletedTask);
            }

            /// <inheritdoc/>
            public Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
                string eventSchema, string contentEncoding, CancellationToken ct) {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public void SendEvent<T>(string target, byte[] data, string contentType,
                string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
                throw new NotImplementedException();
            }
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
