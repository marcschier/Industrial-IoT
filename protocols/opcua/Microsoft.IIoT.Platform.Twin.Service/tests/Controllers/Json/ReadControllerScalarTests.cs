// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Service.Controllers.Json {
    using Microsoft.IIoT.Platform.Twin.Service.Controllers.Test;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.IIoT.Platform.OpcUa;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Utils;
    using Opc.Ua;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [IntegrationTest]
    [Collection(ReadJsonCollection.Name)]
    public class ReadControllerScalarTests : IClassFixture<WebAppFixture> {

        public ReadControllerScalarTests(WebAppFixture factory, TestServerFixture server) {
            _fixture = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private ReadScalarValueTests<string> GetTests() {
            var client = _fixture.CreateClient(); // Call to create server
            var module = _fixture.Resolve<ITestRegistry>();
            module.Connection = Connection;
            var log = _fixture.Resolve<ILogger>();
            var serializer = _fixture.Resolve<IJsonSerializer>();
            return new ReadScalarValueTests<string>(() => // Create an adapter over the api
                new TwinServicesApiAdapter(
                    new ControllerTestClient(new HttpClient(_fixture, log),
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
        public async Task NodeReadStaticScalarBooleanValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarBooleanValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarSByteValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarSByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarByteValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarByteValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarInt16ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarUInt16ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarUInt16ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarInt32ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarUInt32ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarUInt32ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarInt64ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarInt64ValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarUInt64ValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarUInt64ValueVariableTestAsync().ConfigureAwait(false);
        }


        [Fact]
        public async Task NodeReadStaticScalarFloatValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarFloatValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarDoubleValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarDoubleValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarStringValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarDateTimeValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarDateTimeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarGuidValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarGuidValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarByteStringValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarByteStringValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarXmlElementValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarXmlElementValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarNodeIdValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarQualifiedNameValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarLocalizedTextValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarStatusCodeValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarStatusCodeValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarVariantValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarVariantValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarEnumerationValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarEnumerationValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarStructuredValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarStructuredValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarNumberValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarNumberValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarIntegerValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarIntegerValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadStaticScalarUIntegerValueVariableTestAsync() {
            await GetTests().NodeReadStaticScalarUIntegerValueVariableTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadDataAccessMeasurementFloatValueTestAsync() {
            await GetTests().NodeReadDataAccessMeasurementFloatValueTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadDiagnosticsStatusTestAsync() {
            await GetTests().NodeReadDiagnosticsStatusTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadDiagnosticsOperationsTestAsync() {
            await GetTests().NodeReadDiagnosticsStatusTestAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task NodeReadDiagnosticsVerboseTestAsync() {
            await GetTests().NodeReadDiagnosticsStatusTestAsync().ConfigureAwait(false);
        }
    }
}