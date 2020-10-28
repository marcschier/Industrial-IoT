// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service.Api.Binary {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Opc.Ua;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(WriteBinaryCollection.Name)]
    public class CallControllerArrayTests : IClassFixture<WebAppFixture> {

        public CallControllerArrayTests(WebAppFixture factory, TestServerFixture server) {
            _fixture = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private CallArrayMethodTests<string> GetTests() {
            var client = _fixture.CreateClient(); // Call to create server
            var module = _fixture.Resolve<ITestRegistry>();
            module.Connection = Connection;
            var log = _fixture.Resolve<ILogger>();
            var serializer = _fixture.Resolve<IBinarySerializer>();
            return new CallArrayMethodTests<string>(() => // Create an adapter over the api
                new TwinServicesApiAdapter(
                    new TwinServiceClient(new HttpClient(_fixture, log),
                    new TestConfig(client.BaseAddress), serializer)), "fakeid");
        }

        public ConnectionModel Connection => new EndpointModel {
            Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
            AlternativeUrls = _hostEntry?.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
            Certificate = _server.Certificate?.RawData?.ToThumbprint()
        }.ToConnectionModel();

        private readonly WebAppFixture _fixture;
        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;

        [Fact]
        public async Task NodeMethodMetadataStaticArrayMethod1TestAsync() {
            await GetTests().NodeMethodMetadataStaticArrayMethod1TestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodMetadataStaticArrayMethod2TestAsync() {
            await GetTests().NodeMethodMetadataStaticArrayMethod2TestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodMetadataStaticArrayMethod3TestAsync() {
            await GetTests().NodeMethodMetadataStaticArrayMethod3TestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod1Test1Async() {
            await GetTests().NodeMethodCallStaticArrayMethod1Test1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod1Test2Async() {
            await GetTests().NodeMethodCallStaticArrayMethod1Test2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod1Test3Async() {
            await GetTests().NodeMethodCallStaticArrayMethod1Test3Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod1Test4Async() {
            await GetTests().NodeMethodCallStaticArrayMethod1Test4Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod1Test5Async() {
            await GetTests().NodeMethodCallStaticArrayMethod1Test5Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod2Test1Async() {
            await GetTests().NodeMethodCallStaticArrayMethod2Test1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod2Test2Async() {
            await GetTests().NodeMethodCallStaticArrayMethod2Test2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod2Test3Async() {
            await GetTests().NodeMethodCallStaticArrayMethod2Test3Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod2Test4Async() {
            await GetTests().NodeMethodCallStaticArrayMethod2Test4Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod3Test1Async() {
            await GetTests().NodeMethodCallStaticArrayMethod3Test1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod3Test2Async() {
            await GetTests().NodeMethodCallStaticArrayMethod3Test2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeMethodCallStaticArrayMethod3Test3Async() {
            await GetTests().NodeMethodCallStaticArrayMethod3Test3Async().ConfigureAwait(false);
        }

    }
}