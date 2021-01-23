// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Handlers;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa;
    using Microsoft.IIoT.Extensions.LiteDb;
    using Microsoft.IIoT.Extensions.Orleans;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication;
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication.Clients;
    using Microsoft.IIoT.Extensions.AspNetCore.Http.Tunnel;
    using Microsoft.IIoT.Extensions.AspNetCore.SignalR;
    using Microsoft.IIoT.Azure.AppInsights;
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

    /// <summary>
    /// Webservice startup
    /// </summary>
    public class Startup {

        /// <summary>
        /// Configuration - Initialized in constructor
        /// </summary>
        public IConfiguration Configuration { get; }

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
        public Startup(IWebHostEnvironment env, IConfiguration configuration) {
            Environment = env;
            Configuration = configuration;
        }

        /// <summary>
        /// Configure services
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
                Policies.CanWrite,
                Policies.CanManage);

            // TODO: Remove http client factory and use
            // services.AddHttpClient();

            // Add controllers as services so they'll be resolved.
            services.AddControllers().AddSerializers();

            // Add signalr
            services.AddSignalR()
                .AddJsonSerializer()
                .AddMessagePackSerializer();

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
            var log = applicationContainer.Resolve<ILogger<Startup>>();

            app.UsePathBase();
            app.UseHeaderForwarding();

            app.UseRouting();
            app.UseHttpMetrics();
            app.UseCors();

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

            // Connect application servers
            app.RunAppServers();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(applicationContainer.Dispose);

            // Print some useful information at bootstrap time
            log.LogInformation("{service} started with id {id}",
                ServiceInfo.Name, ServiceInfo.Id);
        }

        /// <summary>
        /// Autofac configuration.
        /// </summary>
        /// <param name="builder"></param>
        public virtual void ConfigureContainer(ContainerBuilder builder) {

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

            // --- Logic ---

            // Register services
            builder.RegisterModule<TwinCluster>();
            builder.RegisterModule<DiscoveryRegistry>();
            builder.RegisterModule<PublisherServices>();
            builder.RegisterModule<ClientStack>();
            // Application event hub
            builder.RegisterType<SignalRHub<ApplicationsHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                ApplicationEventForwarder<ApplicationsHub>>()
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

            // Discovery event hub
            builder.RegisterType<SignalRHub<DiscoveryHub>>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<
                DiscoveryProgressForwarder<DiscoveryHub>>()
                .AsImplementedInterfaces().SingleInstance();

            // Handle opc-ua pubsub telemetry subscriptions ...
            builder.RegisterType<DataSetWriterMessageHandler>()
                .AsImplementedInterfaces();

            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add service to service authentication
            builder.RegisterModule<WebApiAuthentication>();
            // Add diagnostics
            builder.AddAppInsightsLogging();
            // Register event bus for integration events
            builder.RegisterModule<OrleansEventBusModule>();
            // Register database for storage
            builder.RegisterModule<LiteDbModule>();
        }
    }
}
