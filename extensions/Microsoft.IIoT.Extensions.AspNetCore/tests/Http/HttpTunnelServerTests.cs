// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Http.Tunnel {
    using Microsoft.IIoT.Extensions.AspNetCore.Tests.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.Tests;
    using Microsoft.IIoT.Extensions.Http;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Net;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class HttpTunnelServerTests : IClassFixture<WebServerFixture> {
        private readonly WebServerFixture _fixture;

        public HttpTunnelServerTests(WebServerFixture fixture) {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestGetRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            // Perform get
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SetAcceptHeaders(httpRequest);
            var httpResponse = await client.GetAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Null(response.Input);
            Assert.Equal("Get", response.Method);
            Assert.Equal("testid", response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPutRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform put
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PutAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();
            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Put", response.Method);
            Assert.NotNull(response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPutAndGetRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform put
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PutAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);
            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Put", response.Method);
            Assert.NotNull(response.Id);

            var id = response.Id;
            uri = new UriBuilder("http", server) {
                Path = $"v2/path/test/{id}"
            }.ToString();
            httpRequest = client.NewRequest(uri);
            serializer.SetAcceptHeaders(httpRequest);
            httpResponse = await client.GetAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Put", response.Method);
            Assert.Equal(id, response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPostRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform post
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PostAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();
            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("testid", response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPutWithBadPathAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();
            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform post
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid" // No id allowed for pu
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PutAsync(httpRequest).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, httpResponse.StatusCode);
            Assert.Throws<InvalidOperationException>(httpResponse.Validate);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPostAndGetRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform post
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PostAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();
            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("testid", response.Id);

            httpRequest = client.NewRequest(uri);
            serializer.SetAcceptHeaders(httpRequest);
            httpResponse = await client.GetAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("testid", response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestPatchAndGetRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform post
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PatchAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            httpRequest = client.NewRequest(uri);
            serializer.SetAcceptHeaders(httpRequest);
            httpResponse = await client.GetAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Patch", response.Method);
            Assert.Equal("testid", response.Id);
        }

        [Theory]
        [MemberData(nameof(WebServerFixture.GetSerializers), MemberType = typeof(WebServerFixture))]
        public async Task TestDeleteRequestAsync(ISerializer serializer) {
            var client = _fixture.Resolve<IHttpClient>();

            var expected = new TestRequestModel {
                Input = "this is a test"
            };

            // Perform post
            var server = HubResource.Format("testhub", "deviceid", "moduleId", true);
            var uri = new UriBuilder("http", server) {
                Path = "v2/path/test/testid"
            }.ToString();
            var httpRequest = client.NewRequest(uri);
            serializer.SerializeToRequest(httpRequest, expected);
            var httpResponse = await client.PostAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();
            var response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Equal(expected.Input, response.Input);
            Assert.Equal("Post", response.Method);
            Assert.Equal("testid", response.Id);

            httpRequest = client.NewRequest(uri);
            httpResponse = await client.DeleteAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            httpRequest = client.NewRequest(uri);
            serializer.SetAcceptHeaders(httpRequest);
            httpResponse = await client.GetAsync(httpRequest).ConfigureAwait(false);
            httpResponse.Validate();

            response = serializer.DeserializeResponse<TestResponseModel>(
                httpResponse);

            Assert.Null(response.Input);
            Assert.Equal("Get", response.Method);
            Assert.Equal("testid", response.Id);
        }

    }
}
