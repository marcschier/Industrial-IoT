// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Supervisor.StartStop {
    using Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Opc.Ua;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Autofac;

    [Collection(ReadCollection.Name)]
    public class SupervisorValueReadScalarTests {

        public SupervisorValueReadScalarTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private ReadScalarValueTests<EndpointInfoModel> GetTests(IContainer services) {
            return new ReadScalarValueTests<EndpointInfoModel>(
                () => services.Resolve<INodeServices<EndpointInfoModel>>(),
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
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadAllStaticScalarVariableNodeClassTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableAccessLevelTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadAllStaticScalarVariableAccessLevelTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableWriteMaskTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadAllStaticScalarVariableWriteMaskTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadAllStaticScalarVariableWriteMaskTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadAllStaticScalarVariableWriteMaskTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarBooleanValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest2Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest3Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarBooleanValueVariableWithBrowsePathTest3Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarSByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarSByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarUInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarUInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarUInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }


        [SkippableFact]
        public async Task NodeReadStaticScalarFloatValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarFloatValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarDoubleValueVariableTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarDoubleValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarDateTimeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarDateTimeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarGuidValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarGuidValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarByteStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarByteStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarXmlElementValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarXmlElementValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarQualifiedNameValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarLocalizedTextValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStatusCodeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarStatusCodeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarVariantValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarVariantValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarEnumerationValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarEnumerationValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarStructuredValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarStructuredValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarNumberValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarNumberValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadStaticScalarUIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadStaticScalarUIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDataAccessMeasurementFloatValueTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadDataAccessMeasurementFloatValueTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsNoneTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadDiagnosticsNoneTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsStatusTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadDiagnosticsStatusTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsOperationsTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadDiagnosticsOperationsTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeReadDiagnosticsVerboseTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeReadDiagnosticsVerboseTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }
    }
}
