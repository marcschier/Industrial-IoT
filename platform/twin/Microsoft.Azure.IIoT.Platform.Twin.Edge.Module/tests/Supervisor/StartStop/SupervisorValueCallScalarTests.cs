// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Supervisor.StartStop {
    using Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Tests;
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
    public class SupervisorValueCallScalarTests {


        public SupervisorValueCallScalarTests(TestServerFixture server) {
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private CallScalarMethodTests<EndpointInfoModel> GetTests(IContainer services) {
            return new CallScalarMethodTests<EndpointInfoModel>(
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
                });
        }

        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;
#if TEST_ALL
        private readonly bool _runAll = true;
#else
        private readonly bool _runAll = System.Environment.GetEnvironmentVariable("TEST_ALL") != null;
#endif

        [SkippableFact]
        public async Task NodeMethodMetadataStaticScalarMethod1TestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodMetadataStaticScalarMethod1TestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticScalarMethod2TestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodMetadataStaticScalarMethod2TestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticScalarMethod3TestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodMetadataStaticScalarMethod3TestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticScalarMethod3WithBrowsePathTest1Async() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodMetadataStaticScalarMethod3WithBrowsePathTest1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticScalarMethod3WithBrowsePathTest2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodMetadataStaticScalarMethod3WithBrowsePathTest2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod1Test1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod1Test1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod1Test2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod1Test2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod1Test3Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod1Test3Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod1Test4Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod1Test4Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod1Test5Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod1Test5Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod2Test1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod2Test1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod2Test2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod2Test2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3Test1Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3Test1Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3Test2Async() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3Test2Async().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3WithBrowsePathNoIdsTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3WithBrowsePathNoIdsTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3WithObjectIdAndBrowsePathTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3WithObjectIdAndBrowsePathTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3WithObjectIdAndMethodIdAndBrowsePathTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3WithObjectIdAndMethodIdAndBrowsePathTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3WithObjectPathAndMethodIdAndBrowsePathTestAsync() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3WithObjectPathAndMethodIdAndBrowsePathTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticScalarMethod3WithObjectIdAndPathAndMethodIdAndPathTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallStaticScalarMethod3WithObjectIdAndPathAndMethodIdAndPathTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallBoiler2ResetTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallBoiler2ResetTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallBoiler1ResetTestAsync() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(async (hub, device, module, services) => {
                    await GetTests(services).NodeMethodCallBoiler1ResetTestAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }
    }
}
