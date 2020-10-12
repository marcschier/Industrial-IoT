// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Supervisor.Endpoint {
    using Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Utils;
    using Opc.Ua;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Autofac;

    [Collection(WriteCollection.Name)]
    public class SupervisorValueWriteScalarTests : IClassFixture<TwinModuleFixture> {

        public SupervisorValueWriteScalarTests(TestServerFixture server, TwinModuleFixture module) {
            _server = server;
            _module = module;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private WriteScalarValueTests<EndpointInfoModel> GetTests() {
            return new WriteScalarValueTests<EndpointInfoModel>(
                () => _module.HubContainer.Resolve<INodeServices<EndpointInfoModel>>(),
                new EndpointInfoModel {
                    Endpoint = new EndpointModel {
                        Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
                        AlternativeUrls = _hostEntry?.AddressList
                            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                            .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
                        Certificate = _server.Certificate?.RawData?.ToThumbprint()
                    },
                    Id = "testid"
                }, (ep, n) => _server.Client.ReadValueAsync(ep.Endpoint, n));
        }

        private readonly TestServerFixture _server;
        private readonly TwinModuleFixture _module;
        private readonly IPHostEntry _hostEntry;

        [Fact]
        public async Task NodeWriteStaticScalarBooleanValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarBooleanValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async() {
            await GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async() {
            await GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async() {
            await GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarSByteValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarSByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarByteValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarInt16ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarUInt16ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarUInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarInt32ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarUInt32ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarUInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarInt64ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarUInt64ValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarUInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarFloatValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarFloatValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarDoubleValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarDoubleValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarStringValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarDateTimeValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarDateTimeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarGuidValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarGuidValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarByteStringValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarByteStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarXmlElementValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarXmlElementValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarNodeIdValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarQualifiedNameValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarLocalizedTextValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarStatusCodeValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarStatusCodeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarVariantValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarVariantValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarEnumerationValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarEnumerationValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarStructuredValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarStructuredValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarNumberValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarNumberValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarIntegerValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarIntegerValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeWriteStaticScalarUIntegerValueVariableTestAsync() {
            await GetTests().NodeWriteStaticScalarUIntegerValueVariableTestAsync().ConfigureAwait(false);
        }
    }
}
