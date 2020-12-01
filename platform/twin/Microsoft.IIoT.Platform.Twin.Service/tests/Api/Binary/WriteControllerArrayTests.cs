// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Service.Api.Binary {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.IIoT.Platform.OpcUa;
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
    [Collection(WriteBinaryCollection.Name)]
    public class WriteControllerArrayTests : IClassFixture<WebAppFixture> {

        public WriteControllerArrayTests(WebAppFixture factory, TestServerFixture server) {
            _fixture = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private WriteArrayValueTests<string> GetTests() {
            var client = _fixture.CreateClient(); // Call to create server
            var module = _fixture.Resolve<ITestRegistry>();
            module.Connection = Connection;
            var log = _fixture.Resolve<ILogger>();
            var serializer = _fixture.Resolve<IBinarySerializer>();
            return new WriteArrayValueTests<string>(() => // Create an adapter over the api
                new TwinServicesApiAdapter(
                    new TwinServiceClient(new HttpClient(_fixture, log),
                    new TestConfig(client.BaseAddress), serializer)), "fakeid",
                    (ep, n) => _server.Client.ReadValueAsync(Connection, n));
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
        public async Task NodeWriteStaticArrayBooleanValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayBooleanValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArraySByteValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArraySByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayByteValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayInt16ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayUInt16ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayUInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayInt32ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayUInt32ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayUInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayInt64ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayUInt64ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayUInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayFloatValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayFloatValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayDoubleValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayDoubleValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayStringValueVariableTest1Async() {
            await GetTests().NodeWriteStaticArrayStringValueVariableTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayStringValueVariableTest2Async() {
            await GetTests().NodeWriteStaticArrayStringValueVariableTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayDateTimeValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayDateTimeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayGuidValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayGuidValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayByteStringValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayByteStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayXmlElementValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayXmlElementValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayNodeIdValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayExpandedNodeIdValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayQualifiedNameValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayLocalizedTextValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayStatusCodeValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayStatusCodeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayVariantValueVariableTest1Async() {
            await GetTests().NodeWriteStaticArrayVariantValueVariableTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayEnumerationValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayEnumerationValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayStructureValueVariableTestAsync() {
            await GetTests().NodeWriteStaticArrayStructureValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayNumberValueVariableTest1Async() {
            await GetTests().NodeWriteStaticArrayNumberValueVariableTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayNumberValueVariableTest2Async() {
            await GetTests().NodeWriteStaticArrayNumberValueVariableTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayIntegerValueVariableTest1Async() {
            await GetTests().NodeWriteStaticArrayIntegerValueVariableTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayIntegerValueVariableTest2Async() {
            await GetTests().NodeWriteStaticArrayIntegerValueVariableTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayUIntegerValueVariableTest1Async() {
            await GetTests().NodeWriteStaticArrayUIntegerValueVariableTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticArrayUIntegerValueVariableTest2Async() {
            await GetTests().NodeWriteStaticArrayUIntegerValueVariableTest2Async().ConfigureAwait(false);
        }

    }
}
