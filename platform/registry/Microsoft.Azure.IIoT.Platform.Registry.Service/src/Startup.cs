// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Service {
    using Microsoft.Azure.IIoT.Platform.Directory.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Directory.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Directory;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Deploy;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Serializers;
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
            this (env, new Config(new ConfigurationBuilder()
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
                Policies.CanQuery,
                Policies.CanManage,
                Policies.CanChange);

            // TODO: Remove http client factory and use
            // services.AddHttpClient();

            // Add controllers as services so they'll be resolved.
            services.AddControllers().AddSerializers();
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
            app.EnableCors();

            app.UseJwtBearerAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirect();

            app.UseSwagger();

            app.UseEndpoints(endpoints => {
                endpoints.MapMetrics();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/healthz");
            });

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

            // Registries and repositories
            builder.RegisterModule<DirectoryServices>();

            // Deployments
            builder.RegisterType<IoTHubConfigurationClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTEdgeBaseDeployment>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<IoTEdgeDiscoveryDeployment>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<IoTEdgeTwinDeployment>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<IoTEdgePublisherDeployment>()
                .AsImplementedInterfaces().SingleInstance();


            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add App Insights logging
            builder.AddAppInsightsLogging(Config);
            // Add service to service authentication
            builder.RegisterModule<WebApiAuthentication>();
            // Iot hub services
            builder.RegisterModule<IoTHubSupportModule>();
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusEventBusSupport>();
        }
    }
}
