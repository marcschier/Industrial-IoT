// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Events.Service {
    using Microsoft.Azure.IIoT.Platform.Edge.Events.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Discovery;
    using Microsoft.Azure.IIoT.Platform.Discovery.Handlers;
    using Microsoft.Azure.IIoT.Platform.Discovery.Events.v2;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Publisher.Handlers;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Azure.CosmosDb;
    using Microsoft.Azure.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.AspNetCore.Diagnostics.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
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
                .ConfigureHostConfiguration(builder => builder
                    .AddFromDotEnvFile()
                    .AddEnvironmentVariables()
                    .AddFromKeyVault())
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostBuilderContext, builder) => 
                    ConfigureContainer(builder, hostBuilderContext.Configuration))
                .ConfigureServices((hostBuilderContext, services) => 
                    services.AddHostedService<HostStarterService>());
        }

        /// <summary>
        /// Autofac configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static ContainerBuilder ConfigureContainer(ContainerBuilder builder,
            IConfiguration configuration) {

            builder.RegisterType<ServiceInfo>()
                .AsImplementedInterfaces();
            builder.AddConfiguration(configuration);
            builder.RegisterType<HostingOptions>()
                .AsImplementedInterfaces();

            // Add prometheus endpoint
            builder.RegisterType<KestrelMetricsHost>()
                .AsImplementedInterfaces().SingleInstance();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            // Add serializers
            builder.RegisterModule<NewtonSoftJsonModule>();

            // --- Logic ---

            // 1.) Handler for discovery progress with publishing to eventbus
            builder.RegisterType<DiscoveryProgressEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscoveryProgressEventBusPublisher>()
                .AsImplementedInterfaces();

            // 2.) Registry storage for data plane control events
            builder.RegisterModule<DiscoveryStorage>();
          //  builder.RegisterType<TwinEventHandler>()
          //      .AsImplementedInterfaces();

            // 3.) Publisher storage for edge events
            builder.RegisterModule<PublisherStorage>();
            builder.RegisterType<WriterGroupEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DataSetWriterEventHandler>()
                .AsImplementedInterfaces();

            // --- Dependencies ---

            // Add Application Insights logging and dependency tracking.
            builder.AddDependencyTracking(new ServiceInfo());
            builder.AddAppInsightsLogging();
            // Add unattended authentication
            builder.RegisterModule<UnattendedAuthentication>();
            // Handle iot hub telemetry events...
            builder.RegisterModule<IoTHubEventsModule>();
            // Register Cosmos db
            builder.RegisterModule<CosmosDbModule>();
            // Event processor services
            builder.RegisterModule<EventHubProcessorModule>();
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusEventBusSupport>();

            return builder;
        }
    }
}
