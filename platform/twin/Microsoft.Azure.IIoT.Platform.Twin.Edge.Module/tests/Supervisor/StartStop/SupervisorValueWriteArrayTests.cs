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
    public class SupervisorValueWriteArrayTests {

        public SupervisorValueWriteArrayTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private WriteArrayValueTests<EndpointInfoModel> GetTests(IContainer services) {
            return new WriteArrayValueTests<EndpointInfoModel>(
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
        public async Task NodeWriteStaticArrayBooleanValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayBooleanValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArraySByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArraySByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayByteValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayByteValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayUInt16ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayUInt16ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayUInt32ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayUInt32ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayUInt64ValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayUInt64ValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayFloatValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayFloatValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayDoubleValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayDoubleValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayStringValueVariableTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayStringValueVariableTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayStringValueVariableTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayStringValueVariableTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayDateTimeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayDateTimeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayGuidValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayGuidValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayByteStringValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayByteStringValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayXmlElementValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayXmlElementValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayExpandedNodeIdValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayExpandedNodeIdValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayQualifiedNameValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayQualifiedNameValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayLocalizedTextValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayLocalizedTextValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayStatusCodeValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayStatusCodeValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayVariantValueVariableTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayVariantValueVariableTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayEnumerationValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayEnumerationValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayStructureValueVariableTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayStructureValueVariableTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayNumberValueVariableTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayNumberValueVariableTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayNumberValueVariableTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayNumberValueVariableTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayIntegerValueVariableTest1Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayIntegerValueVariableTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayIntegerValueVariableTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayIntegerValueVariableTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayUIntegerValueVariableTest1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayUIntegerValueVariableTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeWriteStaticArrayUIntegerValueVariableTest2Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeWriteStaticArrayUIntegerValueVariableTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }
    }
}
