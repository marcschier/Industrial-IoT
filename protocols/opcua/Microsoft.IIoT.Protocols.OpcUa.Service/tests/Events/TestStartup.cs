// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Events {
    using Microsoft.IIoT.Protocols.OpcUa.Service;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Authentication.Models;
    using Microsoft.IIoT.Extensions.Authentication.Runtime;
    using Microsoft.IIoT.Extensions.Http.SignalR.Clients;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Autofac;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.IIoT.Protocols.OpcUa.Api;

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

            // Register test event bus
            builder.RegisterType<DummyProcessingHost>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterModule<MemoryEventBusModule>();

            base.ConfigureContainer(builder);

            // Register api
            builder.AddOpcUa();
            builder.RegisterInstance(new AadApiClientConfig(null))
                .AsImplementedInterfaces().SingleInstance();

            // ... as well as signalR client (needed for api)
            builder.RegisterType<SignalRHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Register client events
            builder.RegisterType<DiscoveryServiceEvents>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TwinServiceEvents>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceEvents>()
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<TestAuthConfig>()
                .AsImplementedInterfaces();
        }

        public class DummyProcessingHost : IEventProcessingHost {

            public Task StartAsync() {
                return Task.CompletedTask;
            }

            public Task StopAsync() {
                return Task.CompletedTask;
            }
        }

        public class TestAuthConfig : IServerAuthConfig, ITokenProvider {
            public bool AllowAnonymousAccess => true;
            public IEnumerable<IOAuthServerConfig> JwtBearerProviders { get; }

            public Task<TokenResultModel> GetTokenForAsync(
                string resource, IEnumerable<string> scopes = null) {
                return Task.FromResult<TokenResultModel>(null);
            }

            public Task InvalidateAsync(string resource) {
                return Task.CompletedTask;
            }

            public bool Supports(string resource) {
                return true;
            }
        }
	}
}
