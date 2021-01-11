// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Tests {
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers.MessagePack;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Autofac.Extensions.Hosting;
    using System.Net.Http;
    using System.Collections.Generic;

    /// <inheritdoc/>
    public class WebServerFixture : WebApplicationFactory<Startup>, IHttpClientFactory {

        /// <summary>
        /// Helper to test multiple serializers in theories
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetSerializers() {
            yield return new object[] { new MessagePackSerializer() };
            yield return new object[] { new NewtonSoftJsonSerializer() };
        }

        /// <inheritdoc/>
        protected override IHostBuilder CreateHostBuilder() {
            return Host.CreateDefaultBuilder();
        }

        /// <inheritdoc/>
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.UseContentRoot(".").UseStartup<Startup>();
            base.ConfigureWebHost(builder);
        }

        /// <inheritdoc/>
        protected override IHost CreateHost(IHostBuilder builder) {
            builder.UseAutofac();
            return base.CreateHost(builder);
        }

        /// <inheritdoc/>
        public HttpClient CreateClient(string name) {
            return CreateClient();
        }

        /// <summary>
        /// Resolve service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() {
            return (T)Server.Services.GetService(typeof(T));
        }
    }
}
