// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service.Controllers.Binary {
    using Microsoft.Azure.IIoT.Platform.Twin.Service.Controllers.Test;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Opc.Ua;
    using Serilog;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(ReadBinaryCollection.Name)]
    public class BrowseControllerTestEx : IClassFixture<WebAppFixture> {

        public BrowseControllerTestEx(WebAppFixture factory, TestServerFixture server) {
            _fixture = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private BrowseServicesTests<string> GetTests() {
            var client = _fixture.CreateClient(); // Call to create server
            var module = _fixture.Resolve<ITestRegistry>();
            module.Connection = Connection;
            var log = _fixture.Resolve<ILogger>();
            var serializer = _fixture.Resolve<IBinarySerializer>();
            return new BrowseServicesTests<string>(() => // Create an adapter over the api
                new TwinServicesApiAdapter(
                    new ControllerTestClient(
                       new HttpClient(_fixture, log), new TestConfig(client.BaseAddress),
                            serializer)), "fakeid");
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
        public async Task NodeBrowseInRootTest2Async() {
            await GetTests().NodeBrowseInRootTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseBoilersObjectsTest1Async() {
            await GetTests().NodeBrowseBoilersObjectsTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest3Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest3Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest4Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest4Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseDataAccessFC1001Test2Async() {
            await GetTests().NodeBrowseDataAccessFC1001Test2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseStaticScalarVariablesTestAsync() {
            await GetTests().NodeBrowseStaticScalarVariablesTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseStaticArrayVariablesTestAsync() {
            await GetTests().NodeBrowseStaticArrayVariablesTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseStaticArrayVariablesWithValuesTestAsync() {
            await GetTests().NodeBrowseStaticArrayVariablesWithValuesTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsStatusTestAsync() {
            await GetTests().NodeBrowseDiagnosticsStatusTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsVerboseTestAsync() {
            await GetTests().NodeBrowseDiagnosticsVerboseTestAsync().ConfigureAwait(false);
        }
    }
}
