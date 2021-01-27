// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Edge.Tunnel.Service {
    using Microsoft.IIoT.Platform.Edge.Tunnel.Service.Runtime;
    using Microsoft.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Azure.IoTHub.Handlers;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.AppInsights;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Http.Tunnel.Services;
    using Microsoft.IIoT.Extensions.Rpc.Services;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.AspNetCore.Diagnostics.Default;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;

    /// <summary>
    /// IoT Hub device tunnel processor host.  Processes all
    /// tunnel requests from devices.
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

            // Handle tunnel server events
            builder.RegisterType<HttpTunnelServer>()
                .AsImplementedInterfaces();
            builder.RegisterType<ChunkMethodClient>()
                .AsImplementedInterfaces();

            // --- Dependencies ---

            // Add Application Insights logging and dependency tracking.
            builder.AddDependencyTracking(new ServiceInfo());
            builder.AddAppInsightsLogging();
            // IoT Hub client and telemetry handler
            builder.RegisterModule<IoTHubSupportModule>();
            builder.RegisterType<IoTHubDeviceEventHandler>()
                .AsImplementedInterfaces();
            // Event processor host
            builder.RegisterModule<EventHubProcessorModule>();
            // Add unattended authentication
            builder.RegisterModule<UnattendedAuthentication>();

            return builder;
        }
    }
}