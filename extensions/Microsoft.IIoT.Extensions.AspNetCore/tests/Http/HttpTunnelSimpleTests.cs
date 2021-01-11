// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Http.Tunnel {
    using Microsoft.IIoT.Extensions.AspNetCore.Tests.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.Tests;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class HttpTunnelSimpleTests : IClassFixture<WebServerFixture> {
        private readonly WebServerFixture _fixture;

        public HttpTunnelSimpleTests(WebServerFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task TestInvokeSimple1Async() {
            var router = _fixture.Resolve<IMethodHandler>();
            var serializer = _fixture.Resolve<IJsonSerializer>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };
            var result = await router.InvokeAsync("target", "v2/path/test/1245",
                serializer.SerializeToBytes(expected).ToArray(), null).ConfigureAwait(false);

            var response = serializer.Deserialize<TestResponseModel>(result);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("1245", response.Id);
        }

        [Fact]
        public async Task TestInvokeSimple2Async() {
            var router = _fixture.Resolve<IMethodHandler>();
            var serializer = _fixture.Resolve<IJsonSerializer>();

            var expected = new TestRequestModel {
                Input = null
            };
            var result = await router.InvokeAsync("target", "v2/path/test/hahahaha",
                serializer.SerializeToBytes(expected).ToArray(), null).ConfigureAwait(false);

            var response = serializer.Deserialize<TestResponseModel>(result);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("hahahaha", response.Id);
        }

        [Fact]
        public async Task TestInvokeSimpleWithBadArgThrowsAsync() {
            var router = _fixture.Resolve<IMethodHandler>();
            var serializer = _fixture.Resolve<IJsonSerializer>();
            var expected = new TestRequestModel {
                Input = "test"
            };

            await Assert.ThrowsAsync<MethodCallStatusException>(
                () => router.InvokeAsync("target", "v2/path/test",
                    serializer.SerializeToBytes(expected).ToArray(), null)).ConfigureAwait(false);

            await Assert.ThrowsAsync<MethodCallStatusException>(
                () => router.InvokeAsync("target", "v2/path/test",
                    null, null)).ConfigureAwait(false);
        }
    }
}
