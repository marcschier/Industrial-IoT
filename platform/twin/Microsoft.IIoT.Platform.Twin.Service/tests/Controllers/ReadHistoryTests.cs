// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Service.Controllers {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Twin.Api;
    using Microsoft.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.IIoT.Platform.Twin.Clients;
    using Microsoft.IIoT.Platform.OpcUa.Services;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Utils;
    using Opc.Ua;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(HistoryReadCollection.Name)]
    public class ReadHistoryTests : IClassFixture<WebAppFixture> {

        public ReadHistoryTests(WebAppFixture factory, HistoryServerFixture server) {
            _fixture = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private HistoryReadValuesTests<string> GetTests() {
            var client = _fixture.CreateClient(); // Call to create server
            var module = _fixture.Resolve<ITestRegistry>();
            module.Connection = Connection;
            var log = _fixture.Resolve<ILogger>();
            var serializer = _fixture.Resolve<IJsonSerializer>();
            return new HistoryReadValuesTests<string>(() => // Create an adapter over the api
                new HistorianServicesAdapter<string>(
                    new HistoryRawAdapter(
                        new TwinServiceClient(new HttpClient(_fixture, log),
                            new TestConfig(client.BaseAddress), serializer)),
                    new VariantEncoderFactory()), "fakeid");
        }

        public ConnectionModel Connection => new EndpointModel {
            Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
            AlternativeUrls = _hostEntry?.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
            Certificate = _server.Certificate?.RawData?.ToThumbprint()
        }.ToConnectionModel();

        private readonly WebAppFixture _fixture;
        private readonly HistoryServerFixture _server;
        private readonly IPHostEntry _hostEntry;

        [Fact]
        public async Task HistoryReadInt64ValuesTest1Async() {
            await GetTests().HistoryReadInt64ValuesTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest2Async() {
            await GetTests().HistoryReadInt64ValuesTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest3Async() {
            await GetTests().HistoryReadInt64ValuesTest3Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest4Async() {
            await GetTests().HistoryReadInt64ValuesTest4Async().ConfigureAwait(false);
        }
    }
}
