// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service {
    using Microsoft.Azure.IIoT.Platform.Api.Events.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Api.Events.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Publisher.Handlers;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Microsoft.Azure.IIoT.Messaging.SignalR.Services;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication.Clients;
    using Microsoft.Azure.IIoT.AspNetCore.Cors;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Prometheus;
    using System;

    /// <summary>
    /// Webservice startup
    /// </summary>
    public class Startup {

        /// <summary>
        /// Configuration - Initialized in constructor
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Service info - Initialized in constructor
        /// </summary>
        public ServiceInfo ServiceInfo { get; } = new ServiceInfo();

        /// <summary>
        /// Current hosting environment - Initialized in constructor
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Create startup
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration) :
            this(env, new Config(new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(EnvironmentVariableTarget.User)
                // Above configuration providers will provide connection
                // details for KeyVault configuration provider.
                .AddFromKeyVault(providerPriority: ConfigurationProviderPriority.Lowest)
                .Build())) {
        }

        /// <summary>
        /// Create startup
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, Config configuration) {
            Environment = env;
            Config = configuration;
        }

        /// <summary>
        /// This is where you register dependencies, add services to the
        /// container. This method is called by the runtime, before the
        /// Configure method below.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public void ConfigureServices(IServiceCollection services) {
            services.AddLogging(o => o.AddConsole().AddDebug());

            services.AddHeaderForwarding();
            services.AddCors();
            services.AddHealthChecks();
            services.AddDistributedMemoryCache();

            services.AddHttpsRedirect();
            services.AddAuthentication()
                .AddJwtBearerProvider(AuthProvider.AzureAD)
                .AddJwtBearerProvider(AuthProvider.AuthService);
            services.AddAuthorizationPolicies(
                Policies.RoleMapping,
                Policies.CanRead,
                Policies.CanWrite);

            // Add controllers as services so they'll be resolved.
            services.AddControllers().AddSerializers();

            // Add signalr and optionally configure signalr service
            services.AddSignalR()
                .AddJsonSerializer()
                .AddMessagePackSerializer()
                .AddAzureSignalRService(Config);

            services.AddSwagger(ServiceInfo.Name, ServiceInfo.Description);

            // Enable Application Insights telemetry collection.
            services.AddAppInsightsTelemetry();
        }

        /// <summary>
        /// This method is called by the runtime, after the ConfigureServices
        /// method above and used to add middleware
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime) {
            var applicationContainer = app.ApplicationServices.GetAutofacRoot();

            app.UsePathBase();
            app.UseHeaderForwarding();

            app.UseRouting();
            app.UseHttpMetrics();
            app.EnableCors();

            app.UseJwtBearerAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirect();

            app.UseSwagger();

            app.UseEndpoints(endpoints => {
                endpoints.MapMetrics();
                endpoints.MapHubs();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/healthz");
            });

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(applicationContainer.Dispose);
        }

        /// <summary>
        /// Autofac configuration.
        /// </summary>
        /// <param name="builder"></param>
        public virtual void ConfigureContainer(ContainerBuilder builder) {

            // Register service info and configuration interfaces
            builder.RegisterInstance(ServiceInfo)
                .AsImplementedInterfaces();
            builder.RegisterInstance(Config)
                .AsImplementedInterfaces();
            builder.RegisterInstance(Config.Configuration)
                .AsImplementedInterfaces();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            // Add serializers
            builder.RegisterModule<MessagePackModule>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            // CORS setup
            builder.RegisterType<CorsSetup>()
                .AsImplementedInterfaces();

            // --- Logic ---

            // Application event hub
            builder.RegisterType<SignalRHub<ApplicationsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                ApplicationEventForwarder<ApplicationsHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Gateways event hub
            builder.RegisterType<SignalRHub<GatewaysHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                GatewayEventForwarder<GatewaysHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Supervisor event hub
            builder.RegisterType<SignalRHub<SupervisorsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                SupervisorEventForwarder<SupervisorsHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Endpoints event hub
            builder.RegisterType<SignalRHub<EndpointsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                EndpointEventForwarder<EndpointsHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Twins event hub
            builder.RegisterType<SignalRHub<TwinsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                TwinEventForwarder<TwinsHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // DataSet Writers event hub
            builder.RegisterType<SignalRHub<WritersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                PublishedDataSetEventForwarder<WritersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                DataSetWriterEventForwarder<WritersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                DataSetWriterMessageForwarder<WritersHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Writer groups event hub
            builder.RegisterType<SignalRHub<GroupsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                WriterGroupEventForwarder<GroupsHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Publishers event hub
            builder.RegisterType<SignalRHub<PublishersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                PublisherEventForwarder<PublishersHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Discovery event hub
            builder.RegisterType<SignalRHub<DiscoverersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                DiscoveryProgressForwarder<DiscoverersHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                DiscovererEventForwarder<DiscoverersHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Handle opc-ua pubsub telemetry subscriptions ...
            builder.RegisterType<DataSetWriterMessageHandler>()
                .AsImplementedInterfaces();

            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add App Insights logging
            builder.AddAppInsightsLogging(Config);
            // Add service to service authentication
            builder.RegisterModule<WebApiAuthentication>();
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusEventBusSupport>();
            // Register event processor host for telemetry
            builder.RegisterModule<EventHubProcessorModule>();
            builder.RegisterType<DeviceEventHandler>()
                .AsImplementedInterfaces();
        }
    }
}