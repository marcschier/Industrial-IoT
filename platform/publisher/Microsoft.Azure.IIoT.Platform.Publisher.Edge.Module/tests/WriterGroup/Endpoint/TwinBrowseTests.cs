// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Twin.Endpoint {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Tests;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Xunit;

    [Collection(PublishCollection.Name)]
    public class TwinBrowseTests : IClassFixture<PublisherModuleFixture> {

        public TwinBrowseTests(TestServerFixture server, PublisherModuleFixture module) {
            _server = server;
            _module = module;
        }

     //   private BrowseServicesTests<string> GetTests() {
     //       var writer = new DataSetWriterModel {
     //           DataSet = new PublishedDataSetModel {
     //               DataSetSource = new PublishedDataSetSourceModel {
     //                   Connection = new ConnectionModel {
     //                       Endpoint = new EndpointModel {
     //                           Url = $"opc.tcp://{Dns.GetHostName()}:{_server.Port}/UA/SampleServer",
     //                           Certificate = _server.Certificate?.RawData?.ToThumbprint()
     //                       }
     //                   }
     //               }
     //           }
     //       };
     //       var writerGroupId = _module.RegisterWriterGroupId(writer);
     //       return new BrowseServicesTests<string>(
     //           () => _module.HubContainer.Resolve<IPublishServices<string>>(), writerGroupId);
     //   }

        private readonly TestServerFixture _server;
        private readonly PublisherModuleFixture _module;

      //  [Fact]
      //  public async Task NodeBrowseInRootTest1Async() {
      //      await GetTests().NodeBrowseInRootTest1Async();
      //  }

    }
}
