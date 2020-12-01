// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Services {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.OpcUa;
    using Microsoft.IIoT.Platform.OpcUa.Services;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.IIoT.Utils;
    using Opc.Ua;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(ReadCollection.Name)]
    public class AddressSpaceValueReadArrayTests {

        public AddressSpaceValueReadArrayTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private ReadArrayValueTests<ConnectionModel> GetTests() {
            return new ReadArrayValueTests<ConnectionModel>(
                () => new AddressSpaceServices(_server.Client,
                    new VariantEncoderFactory(), _server.Logger),
                new EndpointModel {
                    Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
                    AlternativeUrls = _hostEntry?.AddressList
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                        .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer")
                        .ToHashSet(),
                    Certificate = _server.Certificate?.RawData?.ToThumbprint()
                }.ToConnectionModel(), _server.Client.ReadValueAsync);
        }

        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;

        [Fact]
        public async Task NodeReadAllStaticArrayVariableNodeClassTest1Async() {
            await GetTests().NodeReadAllStaticArrayVariableNodeClassTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadAllStaticArrayVariableAccessLevelTest1Async() {
            await GetTests().NodeReadAllStaticArrayVariableAccessLevelTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadAllStaticArrayVariableWriteMaskTest1Async() {
            await GetTests().NodeReadAllStaticArrayVariableWriteMaskTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadAllStaticArrayVariableWriteMaskTest2Async() {
            await GetTests().NodeReadAllStaticArrayVariableWriteMaskTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayBooleanValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayBooleanValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArraySByteValueVariableTestAsync() {
            await GetTests().NodeReadStaticArraySByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayByteValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayInt16ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayUInt16ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayUInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayInt32ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayUInt32ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayUInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayInt64ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayUInt64ValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayUInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayFloatValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayFloatValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayDoubleValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayDoubleValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayStringValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayDateTimeValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayDateTimeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayGuidValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayGuidValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayByteStringValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayByteStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayXmlElementValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayXmlElementValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayNodeIdValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayExpandedNodeIdValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayQualifiedNameValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayLocalizedTextValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayStatusCodeValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayStatusCodeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayVariantValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayVariantValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayEnumerationValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayEnumerationValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayStructureValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayStructureValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayNumberValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayNumberValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayIntegerValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayIntegerValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticArrayUIntegerValueVariableTestAsync() {
            await GetTests().NodeReadStaticArrayUIntegerValueVariableTestAsync().ConfigureAwait(false);
        }
    }
}
