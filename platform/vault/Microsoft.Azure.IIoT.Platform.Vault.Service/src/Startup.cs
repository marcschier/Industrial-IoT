// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Service {
    using Microsoft.Azure.IIoT.Platform.Vault.Events;
    using Microsoft.Azure.IIoT.Platform.Vault.Handler;
    using Microsoft.Azure.IIoT.Platform.Vault.Services;
    using Microsoft.Azure.IIoT.Platform.Vault.Storage;
    using Microsoft.Azure.IIoT.Platform.Vault.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Vault.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Registry.Events.v2;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Azure.CosmosDb;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication.Clients;
    using Microsoft.Azure.IIoT.AspNetCore.Correlation;
    using Microsoft.Azure.IIoT.AspNetCore.Cors;
    using Microsoft.Azure.IIoT.Auth;
    using Microsoft.Azure.IIoT.Cryptography.Default;
    using Microsoft.Azure.IIoT.Cryptography.Storage;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Prometheus;
    using System;
    using ILogger = Serilog.ILogger;

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

            // services.AddLogging(o => o.AddConsole().AddDebug());

            services.AddHeaderForwarding();
            services.AddCors();
            services.AddHealthChecks();
            services.AddDistributedMemoryCache();
            services.AddAzureDataProtection(Config.Configuration);

            services.AddHttpsRedirect();
            services.AddAuthentication()
                .AddJwtBearerProvider(AuthProvider.AzureAD)
                .AddJwtBearerProvider(AuthProvider.AuthService);
            services.AddAuthorizationPolicies(
                Policies.RoleMapping,
                Policies.CanRead,
                Policies.CanWrite,
                Policies.CanSign,
                Policies.CanManage);

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
            var log = applicationContainer.Resolve<ILogger>();

            app.UsePathBase();
            app.UseHeaderForwarding();

            app.UseRouting();
            app.UseHttpMetrics();
            app.EnableCors();

            app.UseJwtBearerAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirect();

            app.UseCorrelation();
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
            log.Information("{service} web service started with id {id}",
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

            // Crypto services
            builder.RegisterType<CertificateDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateRevoker>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateFactory>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateIssuer>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KeyDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KeyHandleSerializer>()
                .AsImplementedInterfaces().SingleInstance();

            // Register registry micro services adapters
            builder.RegisterType<RegistryServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<RegistryServicesApiAdapter>()
                .AsImplementedInterfaces();
            builder.RegisterType<EntityInfoResolver>()
                .AsImplementedInterfaces();

            // Vault services
            builder.RegisterType<RequestDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GroupDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateRequestEventBroker>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateRequestManager>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TrustGroupServices>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateAuthority>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KeyPairRequestProcessor>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SigningRequestProcessor>()
                .AsImplementedInterfaces().SingleInstance();

            // ... subscribe to application events ...
            builder.RegisterType<ApplicationEventBusSubscriber>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RegistryEventHandler>()
                .AsImplementedInterfaces();

            // Vault handler
            builder.RegisterType<AutoApproveHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<KeyPairRequestHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<SigningRequestHandler>()
                .AsImplementedInterfaces();

            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add service to service authentication
            builder.RegisterModule<WebApiAuthentication>();
            // Add diagnostics
            builder.AddAppInsightsLogging(Config);
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusModule>();
            // ... with cosmos db collection as storage
            builder.RegisterModule<CosmosDbModule>();
            // Add key vault  - TODO - enable
            // builder.RegisterModule<KeyVaultAuthentication>();
            // builder.RegisterModule<KeyVaultClientModule>();
        }
    }
}
