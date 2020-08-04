// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Subscriber.Service {
    using Microsoft.Azure.IIoT.Platform.Subscriber.Service.Runtime;
    using Microsoft.Azure.IIoT.Platform.Subscriber.Handlers;
    using Microsoft.Azure.IIoT.Platform.Subscriber.Processors;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.EventHub;
    using Microsoft.Azure.IIoT.Azure.EventHub.Runtime;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Handlers;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.AspNetCore.Diagnostics.Default;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// IoT Hub device telemetry event processor host.  Processes all
    /// telemetry from devices - forwards unknown telemetry on to
    /// time series event hub.
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
            // Add serializers
            builder.RegisterModule<NewtonSoftJsonModule>();

            // --- Logic ---

            // Handle opc-ua pub/sub subscriber messages
            builder.RegisterType<VariantEncoderFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<MonitoredItemSampleBinaryHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<MonitoredItemSampleJsonHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<PubSubNetworkMessageBinaryHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<PubSubNetworkMessageJsonHandler>()
                .AsImplementedInterfaces();

            // ... and forward results and unknowns to secondary queue
            builder.RegisterType<MonitoredItemSampleForwarder>()
                .AsImplementedInterfaces();
            builder.RegisterType<UnknownTelemetryForwarder>()
                .AsImplementedInterfaces();

            // --- Dependencies ---

            // Add Application Insights logging and dependency tracking.
            builder.AddDependencyTracking(config, serviceInfo);
            builder.AddAppInsightsLogging(config);
            // Event Hub client
            builder.RegisterType<EventHubClientConfig>()
                .AsImplementedInterfaces();
            builder.RegisterModule<EventHubModule>();
            // Event processor host
            builder.RegisterModule<EventHubProcessorModule>();
            // Handle telemetry events
            builder.RegisterType<IoTHubDeviceEventHandler>()
                .AsImplementedInterfaces();

            return builder;
        }

        /// <summary>
        /// Forwards telemetry not part of the platform for example from other devices
        /// </summary>
        internal sealed class UnknownTelemetryForwarder : IUnknownEventProcessor {

            /// <summary>
            /// Create forwarder
            /// </summary>
            /// <param name="client"></param>
            public UnknownTelemetryForwarder(IEventQueueClient client) {
                _client = client ?? throw new ArgumentNullException(nameof(client));
            }

            /// <inheritdoc/>
            public async Task HandleAsync(byte[] eventData, IDictionary<string, string> properties) {
                properties.TryGetValue(SystemProperties.To, out var route);
                await _client.SendAsync(route, eventData, properties);
            }

            private readonly IEventQueueClient _client;
        }
    }
}
