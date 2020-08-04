// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Service {
    using Microsoft.Azure.IIoT.Platform.Discovery.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Discovery.Services;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Clients;
    using Microsoft.Azure.IIoT.Platform.Discovery.Handlers;
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Handlers;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Deploy;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Azure.ServiceBus;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.Rpc.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.AspNetCore.Diagnostics.Default;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Serilog;
    using System;

    /// <summary>
    /// IoT Hub device onboarding processor host.  Processes all
    /// events from devices including onboarding and discovery events.
    /// </summary>
    public class Program {

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

            // Handle discovery events and process them
            builder.RegisterType<DiscoveryEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscoveryProcessor>()
                .AsImplementedInterfaces();

            // Use registry services directly
            builder.RegisterModule<RegistryServices>();
            builder.RegisterType<ApplicationTwins>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TwinModuleCertificateClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinModuleActivationClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<OnboardingClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<ChunkMethodClient>()
                .AsImplementedInterfaces();

            // Deployments
            builder.RegisterType<IoTHubConfigurationClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTEdgeBaseDeployment>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<IoTEdgeDiscovererDeployment>()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add Application Insights logging and dependency tracking.
            builder.AddDependencyTracking(config, serviceInfo);
            builder.AddAppInsightsLogging(config);
            // IoT Hub and device events
            builder.RegisterType<LogAnalyticsConfig>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterModule<IoTHubModule>();
            builder.RegisterType<IoTHubDeviceEventHandler>()
                .AsImplementedInterfaces();
            // Event processor host
            builder.RegisterModule<EventHubProcessorModule>();
            // Register event bus for integration events
            builder.RegisterModule<ServiceBusModule>();

            return builder;
        }
    }
}
