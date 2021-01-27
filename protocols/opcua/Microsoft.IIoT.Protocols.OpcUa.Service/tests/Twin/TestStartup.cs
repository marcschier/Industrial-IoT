// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Protocols.OpcUa.Testing.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Autofac;
    using System.Collections.Generic;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.AspNetCore.Http.Tunnel;
    using Microsoft.IIoT.Extensions.LiteDb;
    using Microsoft.IIoT.Extensions.Utils;

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

            builder.AddDiagnostics();
            // Register service info and configuration interfaces
            builder.RegisterInstance(ServiceInfo)
                .AsImplementedInterfaces();
            builder.AddConfiguration(Configuration);
            builder.RegisterType<HostingOptions>()
                .AsImplementedInterfaces();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            // Add serializers
            builder.RegisterModule<MessagePackModule>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            // Http tunnel
            builder.RegisterType<HttpTunnelServer>()
                .AsImplementedInterfaces().SingleInstance();

            // Add fakes
            builder.RegisterType<TestRegistry>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TestClientServicesConfig>()
               .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TestAuthConfig>()
                .AsImplementedInterfaces();

            builder.RegisterModule<MemoryEventBusModule>();
            builder.RegisterModule<TwinServices>();
            builder.RegisterModule<ClientStack>();

            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();
        }

        public class TestAuthConfig : IServerAuthConfig {
            public bool AllowAnonymousAccess => true;
            public IEnumerable<IOAuthServerConfig> JwtBearerProviders { get; }
        }
    }
}
