// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Events.Service {
    using Microsoft.Azure.IIoT.Platform.Edge.Events.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Handlers;
    using Microsoft.Azure.IIoT.Platform.Registry.Events.v2;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Azure.CosmosDb;
    using Microsoft.Azure.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.AspNetCore.Diagnostics.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Serilog;
    using System;

    /// <summary>
    /// IoT Hub device events event processor host.  Processes all
    /// events from devices including onboarding and discovery events.
    /// </summary>
    public static class Program {

        /// <summary>
        /// Main entry point for iot hub device event processor host
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost => {
                    configHost.AddFromDotEnvFile()
                    .AddEnvironmentVariables()
                    .AddEnvironmentVariables(EnvironmentVariableTarget.User)
                    // Above configuration providers will provide connection
                    // details for KeyVault configuration provider.
                    .AddFromKeyVault(providerPriority: ConfigurationProviderPriority.Lowest);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostBuilderContext, builder) => {
                    // registering services in the Autofac ContainerBuilder
                    ConfigureContainer(builder, hostBuilderContext.Configuration);
                })
                .ConfigureServices((hostBuilderContext, services) => {
                    services.AddHostedService<HostStarterService>();
                })
                .UseSerilog();
        }

        /// <summary>
        /// Autofac configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static ContainerBuilder ConfigureContainer(ContainerBuilder builder,
            IConfiguration configuration) {

            var serviceInfo = new ServiceInfo();
            var config = new Config(configuration);

            // Making sure that we reuse the same ServiceInfo instance.
            builder.RegisterInstance(serviceInfo)
                .AsImplementedInterfaces();

            // Register configuration interfaces
            builder.RegisterInstance(config)
                .AsSelf()
                .AsImplementedInterfaces();
            builder.RegisterInstance(config.Configuration)
                .AsImplementedInterfaces();

            // Add prometheus endpoint
            builder.RegisterType<KestrelMetricsHost>()
                .AsImplementedInterfaces().SingleInstance();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            // Add serializers
            builder.RegisterModule<NewtonSoftJsonModule>();

            // --- Logic ---

            // 1.) Handler for discovery progress
            builder.RegisterType<DiscoveryProgressHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscoveryProgressEventBusPublisher>()
                .AsImplementedInterfaces();

            // 2.) Handlers for twin and device change events ...
            builder.RegisterModule<RegistryTwinEventHandlers>();

            // 3.) Publisher events
            // TODO: because of dependencies should be in seperate processor
            builder.RegisterType<WriterGroupTwinEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<WriterGroupEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterModule<PublisherServices>();

            // --- Dependencies ---

            // Add Application Insights logging and dependency tracking.
            builder.AddDependencyTracking(config, serviceInfo);
            builder.AddAppInsightsLogging(config);
            // Add unattended authentication
            builder.RegisterModule<UnattendedAuthentication>();
            // Handle iot hub telemetry events...
            builder.RegisterModule<IoTHubEventsModule>();
            // Register Cosmos db
            builder.RegisterModule<CosmosDbModule>();
            // Event processor services
            builder.RegisterModule<EventHubProcessorModule>();
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusModule>();

            return builder;
        }
    }
}
