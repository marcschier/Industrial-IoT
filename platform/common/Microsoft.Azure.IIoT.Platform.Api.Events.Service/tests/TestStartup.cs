// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service {
    using Microsoft.Azure.IIoT.Platform.Publisher.Api;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api;
    using Microsoft.Azure.IIoT.Platform.Twin.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Events.Api.Runtime;
    using Microsoft.Azure.IIoT.Authentication.Runtime;
    using Microsoft.Azure.IIoT.Authentication.Models;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Http.SignalR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Autofac;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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

            // Register events api configuration interface
            builder.RegisterType<EventsConfig>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new AadApiClientConfig(null))
                .AsImplementedInterfaces().SingleInstance();

            // ... as well as signalR client (needed for api)
            builder.RegisterType<SignalRHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Register client events
            builder.RegisterType<RegistryServiceEvents>()
                .AsImplementedInterfaces().SingleInstance();
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
