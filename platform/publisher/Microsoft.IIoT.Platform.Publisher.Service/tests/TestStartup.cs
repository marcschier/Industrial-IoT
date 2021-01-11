// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Service {
    using Microsoft.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers.MessagePack;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using Autofac.Extensions.Hosting;
    using System.Net.Http;
    using System.Collections.Generic;

    /// <summary>
    /// Startup class for tests
    /// </summary>
    public class TestStartup : Startup {

        /// <inheritdoc/>
        public TestStartup(IWebHostEnvironment env, IConfiguration configuration) :
            base(env, configuration) {
        }

        /// <inheritdoc/>
        public override void ConfigureContainer(ContainerBuilder builder) {
            builder.RegisterModule<MemoryEventBusModule>();
            base.ConfigureContainer(builder);

            // Add fakes
            //  builder.RegisterType<TestRegistry>()
            //      .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TestClientServicesConfig>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TestAuthConfig>()
                .AsImplementedInterfaces();
        }

        public class TestAuthConfig : IServerAuthConfig {
            public bool AllowAnonymousAccess => true;
            public IEnumerable<IOAuthServerConfig> JwtBearerProviders { get; }
        }
    }

    /// <inheritdoc/>
    public class WebAppFixture : WebApplicationFactory<TestStartup>, IHttpClientFactory {

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
            builder.UseContentRoot(".").UseStartup<TestStartup>();
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
