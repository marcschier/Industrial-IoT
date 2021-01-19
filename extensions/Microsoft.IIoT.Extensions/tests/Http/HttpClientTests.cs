// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Http {
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class HttpClientTests {

        [Fact]
        public void UnixDomainSocketHttpRequestTest1() {
            var logger = Log.Console();
            using var factory = new HttpClientFactory(logger);
            IHttpClient client = new HttpClient(factory, logger);
            var request = client.NewRequest(new Uri("unix:///var/test/unknown.sock/path/to/resource?query=36"));

            Assert.True(request.Headers.Contains(HttpHeader.UdsPath));
            var path = request.Headers.GetValues(HttpHeader.UdsPath).First();
            Assert.Equal("/var/test/unknown.sock", path);
            Assert.Equal("/path/to/resource", request.Uri.LocalPath);
            Assert.Equal("/path/to/resource?query=36", request.Uri.PathAndQuery);
        }

        [Fact]
        public void UnixDomainSocketHttpRequestTest2() {
            var logger = Log.Console();
            using var factory = new HttpClientFactory(logger);
            IHttpClient client = new HttpClient(factory, logger);
            var request = client.NewRequest(new Uri("unix:///var/test/unknown.sock:0/path/to/resource?query=36"));

            Assert.True(request.Headers.Contains(HttpHeader.UdsPath));
            var path = request.Headers.GetValues(HttpHeader.UdsPath).First();
            Assert.Equal("/var/test/unknown.sock", path);
            Assert.Equal("/path/to/resource?query=36", request.Uri.PathAndQuery);
        }

        [Fact]
        public void UnixDomainSocketHttpRequestTest2b() {
            var logger = Log.Console();
            using var factory = new HttpClientFactory(logger);
            IHttpClient client = new HttpClient(factory, logger);
            var request = client.NewRequest(new Uri("unix:///var/test/unknown.sock:0/path/to/resource"));

            Assert.True(request.Headers.Contains(HttpHeader.UdsPath));
            var path = request.Headers.GetValues(HttpHeader.UdsPath).First();
            Assert.Equal("/var/test/unknown.sock", path);
            Assert.Equal("/path/to/resource", request.Uri.PathAndQuery);
        }

        [Fact]
        public void UnixDomainSocketHttpRequestTest3() {
            var logger = Log.Console();
            using var factory = new HttpClientFactory(logger);
            IHttpClient client = new HttpClient(factory, logger);
            var request = client.NewRequest(new Uri("unix:///var/test/unknown:0/path/to/resource?query=36"));

            Assert.True(request.Headers.Contains(HttpHeader.UdsPath));
            var path = request.Headers.GetValues(HttpHeader.UdsPath).First();
            Assert.Equal("/var/test/unknown", path);
            Assert.Equal("/path/to/resource?query=36", request.Uri.PathAndQuery);
        }

        [Fact]
        public async Task UnixDomainSocketHttpClientTestAsync() {
            var logger = Log.Console();
            using var factory = new HttpClientFactory(logger);
            IHttpClient client = new HttpClient(factory, logger);
            var request = client.NewRequest(new Uri("unix:///var/test/unknown.sock:0/path/to/resource?query=36"));
            try {
                await client.GetAsync(request).ConfigureAwait(false);
                Assert.True(false);
            }
            catch (SocketException ex) {
                Assert.True(true);
                Assert.NotNull(ex);
            }
            catch {
                Assert.True(false);
            }
        }
    }
}