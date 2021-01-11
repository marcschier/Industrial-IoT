// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Services {
    using Microsoft.IIoT.Platform.Twin.Clients;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.OpcUa.Services;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.IIoT.Extensions.Utils;
    using Opc.Ua;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Xunit;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(HistoryReadCollection.Name)]
    public class AddressSpaceHistoryReadTests {

        public AddressSpaceHistoryReadTests(HistoryServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private HistoryReadValuesTests<ConnectionModel> GetTests() {
            var codec = new VariantEncoderFactory();
            return new HistoryReadValuesTests<ConnectionModel>(
                () => new HistorianServicesAdapter<ConnectionModel>(new AddressSpaceServices(_server.Client,
                    codec, _server.Logger), codec),
                new EndpointModel {
                    Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
                    AlternativeUrls = _hostEntry?.AddressList
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                        .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
                    Certificate = _server.Certificate?.RawData?.ToThumbprint()
                }.ToConnectionModel());
        }

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

        private readonly HistoryServerFixture _server;
        private readonly IPHostEntry _hostEntry;
    }
}
