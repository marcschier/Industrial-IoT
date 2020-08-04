// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.History.Endpoint {
    using Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Platform.History;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;
    using Autofac;

    [Collection(ReadHistoryCollection.Name)]
    public class TwinReadValuesTests : IClassFixture<TwinModuleFixture> {

        public TwinReadValuesTests(HistoryServerFixture server, TwinModuleFixture module) {
            _server = server;
            _module = module;
        }

        private HistoryReadValuesTests<string> GetTests() {
            var endpoint = new EndpointRegistrationModel {
                Endpoint = new EndpointModel {
                    Url = $"opc.tcp://{Dns.GetHostName()}:{_server.Port}/UA/SampleServer",
                    Certificate = _server.Certificate?.RawData?.ToThumbprint()
                },
                Id = "testid",
                SupervisorId = SupervisorModelEx.CreateSupervisorId(
                  _module.DeviceId, _module.ModuleId)
            };
            endpoint = _module.RegisterAndActivateTwinId(endpoint);
            return new HistoryReadValuesTests<string>(
                () => _module.HubContainer.Resolve<IHistorianServices<string>>(), endpoint.Id);
        }

        private readonly HistoryServerFixture _server;
        private readonly TwinModuleFixture _module;


        [Fact]
        public async Task HistoryReadInt64ValuesTest1Async() {
            await GetTests().HistoryReadInt64ValuesTest1Async();
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest2Async() {
            await GetTests().HistoryReadInt64ValuesTest2Async();
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest3Async() {
            await GetTests().HistoryReadInt64ValuesTest3Async();
        }

        [Fact]
        public async Task HistoryReadInt64ValuesTest4Async() {
            await GetTests().HistoryReadInt64ValuesTest4Async();
        }
    }
}
