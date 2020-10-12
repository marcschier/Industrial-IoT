// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Twin.StartStop {
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

    [Collection(ReadCollection.Name)]
    public class TwinValueReadScalarTests {

        public TwinValueReadScalarTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }


        private EndpointModel Endpoint => new EndpointModel {
            Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
            AlternativeUrls = _hostEntry?.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
            Certificate = _server.Certificate?.RawData?.ToThumbprint()
        };

        private ReadScalarValueTests<string> GetTests(EndpointInfoModel endpoint, IContainer services) {
            return new ReadScalarValueTests<string>(
                () => services.Resolve<INodeServices<string>>(), endpoint.Id,
                (ep, n) => _server.Client.ReadValueAsync(endpoint.Endpoint, n));
        }

        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;
#if TEST_ALL
        private readonly bool _runAll = true;
#else
        private readonly bool _runAll = System.Environment.GetEnvironmentVariable("TEST_ALL") != null;
#endif

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableNodeClassTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadAllStaticScalarVariableNodeClassTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableAccessLevelTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadAllStaticScalarVariableAccessLevelTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableWriteMaskTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadAllStaticScalarVariableWriteMaskTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableWriteMaskTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadAllStaticScalarVariableWriteMaskTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarBooleanValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest2Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest3Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest3Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarSByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarSByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarUInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarUInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarUInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }


        [SkippableFact]
        public async Task NodeReadStaticScalarFloatValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarFloatValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarDoubleValueVariableTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarDoubleValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarDateTimeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarDateTimeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarGuidValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarGuidValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarByteStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarByteStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarXmlElementValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarXmlElementValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarQualifiedNameValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarLocalizedTextValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStatusCodeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarStatusCodeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarVariantValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarVariantValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarEnumerationValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarEnumerationValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStructuredValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarStructuredValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarNumberValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarNumberValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadStaticScalarUIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDataAccessMeasurementFloatValueTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadDataAccessMeasurementFloatValueTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }


        [SkippableFact]
        public async Task NodeReadDiagnosticsNoneTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadDiagnosticsNoneTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsStatusTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadDiagnosticsStatusTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsOperationsTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadDiagnosticsOperationsTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsVerboseTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeReadDiagnosticsVerboseTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }
    }
}
