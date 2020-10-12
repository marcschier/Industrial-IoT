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

    [Collection(WriteCollection.Name)]
    public class SupervisorValueWriteScalarTests {

        public SupervisorValueWriteScalarTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private WriteScalarValueTests<EndpointInfoModel> GetTests(IContainer services) {
            return new WriteScalarValueTests<EndpointInfoModel>(
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
        public async Task NodeWriteStaticScalarBooleanValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarBooleanValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarSByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarSByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarUInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarUInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarUInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarUInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarUInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarUInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarFloatValueVariableTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarFloatValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarDoubleValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarDoubleValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarDateTimeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarDateTimeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarGuidValueVariableTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarGuidValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarByteStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarByteStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarXmlElementValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarXmlElementValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarQualifiedNameValueVariableTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarLocalizedTextValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarStatusCodeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarStatusCodeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarVariantValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarVariantValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarEnumerationValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarEnumerationValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarStructuredValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarStructuredValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarNumberValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarNumberValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticScalarUIntegerValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticScalarUIntegerValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }
    }
}
