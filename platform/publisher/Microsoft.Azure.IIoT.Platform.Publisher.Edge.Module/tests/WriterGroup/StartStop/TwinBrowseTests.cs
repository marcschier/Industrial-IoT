// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Twin.StartStop {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using System.Net;
    using Xunit;
    using Autofac;

    [Collection(PublishCollection.Name)]
    public class TwinBrowseTests {

        public TwinBrowseTests(TestServerFixture server) {
            _server = server;
        }

        private EndpointModel Endpoint => new EndpointModel {
            Url = $"opc.tcp://{Dns.GetHostName()}:{_server.Port}/UA/SampleServer",
            Certificate = _server.Certificate?.RawData?.ToThumbprint()
        };

        private BrowseServicesTests<string> GetTests(EndpointRegistrationModel endpoint,
            IContainer services) {
            return new BrowseServicesTests<string>(
                () => services.Resolve<IBrowseServices<string>>(), endpoint.Id);
        }

        private readonly TestServerFixture _server;
#if TEST_ALL
        private readonly bool _runAll = true;
#else
        private readonly bool _runAll = System.Environment.GetEnvironmentVariable("TEST_ALL") != null;
#endif

      // [SkippableFact]
      // public async Task NodeBrowseInRootTest1Async() {
      //     // Skip.IfNot(_runAll);
      //     using (var harness = new PublisherModuleFixture()) {
      //         await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
      //             await GetTests(endpoint, services).NodeBrowseInRootTest1Async();
      //         });
      //     }
      // }

    }
}
