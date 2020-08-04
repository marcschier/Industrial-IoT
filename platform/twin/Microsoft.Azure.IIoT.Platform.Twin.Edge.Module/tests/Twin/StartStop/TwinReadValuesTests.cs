// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.History.StartStop {
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
    public class TwinReadValuesTests {

        public TwinReadValuesTests(HistoryServerFixture server) {
            _server = server;
        }

        private EndpointModel Endpoint => new EndpointModel {
            Url = $"opc.tcp://{Dns.GetHostName()}:{_server.Port}/UA/SampleServer",
            Certificate = _server.Certificate?.RawData?.ToThumbprint()
        };

        private HistoryReadValuesTests<string> GetTests(EndpointRegistrationModel endpoint,
            IContainer services) {
            return new HistoryReadValuesTests<string>(
                () => services.Resolve<IHistorianServices<string>>(), endpoint.Id);
        }

        private readonly HistoryServerFixture _server;
#if TEST_ALL
        private readonly bool _runAll = true;
#else
        private readonly bool _runAll = System.Environment.GetEnvironmentVariable("TEST_ALL") != null;
#endif

        [SkippableFact]
        public async Task HistoryReadInt64ValuesTest1Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).HistoryReadInt64ValuesTest1Async();
                });
            }
        }

        [SkippableFact]
        public async Task HistoryReadInt64ValuesTest2Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).HistoryReadInt64ValuesTest2Async();
                });
            }
        }

        [SkippableFact]
        public async Task HistoryReadInt64ValuesTest3Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).HistoryReadInt64ValuesTest3Async();
                });
            }
        }

        [SkippableFact]
        public async Task HistoryReadInt64ValuesTest4Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).HistoryReadInt64ValuesTest4Async();
                });
            }
        }
    }
}
