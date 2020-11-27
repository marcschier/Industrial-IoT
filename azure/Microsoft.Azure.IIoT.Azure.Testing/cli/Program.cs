// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Testing.Cli {
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.Testing.IoTEdge;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Extensions.Mqtt;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using System.Threading;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using System.Net;

    /// <summary>
    /// Networking command line interface
    /// </summary>
    public class Program {

        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args) {
            RunAsync(args).Wait();
        }

        /// <summary>
        /// Run cli
        /// </summary>
        public static async Task RunAsync(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }

            var interactive = false;
            try {
                do {
                    if (interactive) {
                        Console.Write("> ");
                        args = CliOptions.ParseAsCommandLine(Console.ReadLine());
                    }
                    try {
                        if (args.Length < 1) {
                            throw new ArgumentException("Need a command!");
                        }

                        CliOptions options;
                        var command = args[0];
                        switch (command) {
                            case "exit":
                                interactive = false;
                                break;
                            case "console":
                                Console.WriteLine("Azure Tester");
                                interactive = true;
                                break;
                            case "iotedge":
                                if (args.Length < 2) {
                                    throw new ArgumentException("Need a command!");
                                }
                                command = args[1];
                                options = new CliOptions(args, 2);
                                switch (command) {
                                    case "test":
                                        await TestMqttClientAsync(options).ConfigureAwait(false);
                                        break;
                                    case "start":
                                        await StartIoTEdgeAsync(options).ConfigureAwait(false);
                                        break;
                                    case "stop":
                                        await StopIoTEdgeAsync(options).ConfigureAwait(false);
                                        break;
                                    case "-?":
                                    case "-h":
                                    case "--help":
                                    case "help":
                                        PrintHelp();
                                        break;
                                    default:
                                        throw new ArgumentException($"Unknown command {command}.");
                                }
                                break;
                            case "-?":
                            case "-h":
                            case "--help":
                            case "help":
                                PrintHelp();
                                break;
                            default:
                                throw new ArgumentException($"Unknown command {command}.");
                        }
                    }
                    catch (ArgumentException e) {
                        Console.WriteLine(e.Message);
                        if (!interactive) {
                            PrintHelp();
                            return;
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("==================");
                        Console.WriteLine(e);
                        Console.WriteLine("==================");
                    }
                }
                while (interactive);
            }
            finally {
                await CleanupAsync().ConfigureAwait(false);
            }
        }

        private static readonly Dictionary<string, IoTEdgeDevice> kEdges = new();

        /// <summary>
        /// Stop an IoT edge
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static async Task CleanupAsync() {
            List<IoTEdgeDevice> edges;
            lock (kEdges) {
                edges = kEdges.Values.ToList();
                kEdges.Clear();
            }
            foreach (var edge in edges) {
                await edge.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Stop an IoT edge
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static async Task StopIoTEdgeAsync(CliOptions options) {
            IoTEdgeDevice container = null;
            lock (kEdges) {
                var deviceId = options.GetValue<string>("-d", "--deviceId");
                if (!kEdges.TryGetValue(deviceId, out container)) {
                    return;
                }
                kEdges.Remove(deviceId);
            }
            await container.StopAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Start an IoT edge
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static async Task StartIoTEdgeAsync(CliOptions options) {
            IoTEdgeDevice container = null;
            lock (kEdges) {
                var deviceId = options.GetValue<string>("-d", "--deviceId");
                if (kEdges.ContainsKey(deviceId)) {
                    return;
                }
                var logger = Log.Console(LogLevel.Error);
                var configuration = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddEnvironmentVariables()
                    .AddFromKeyVault()
                    .Build();
                var iotHubCs = configuration.GetValue<string>(PcsVariable.PCS_IOTHUB_CONNSTRING, null);
                if (string.IsNullOrEmpty(iotHubCs)) {
                    iotHubCs = configuration.GetValue<string>("_HUB_CS", null);
                }
                if (string.IsNullOrEmpty(iotHubCs)) {
                    throw new ArgumentException("Missing connection string.");
                }
                if (!ConnectionString.TryParse(iotHubCs, out var connectionString)) {
                    throw new ArgumentException("Bad connection string.");
                }
                var config = connectionString.ToIoTHubOptions();
                var registry = new IoTHubServiceClient(
                    config, new NewtonSoftJsonSerializer(), logger);
                container = new IoTEdgeDevice(registry, deviceId, logger);
                kEdges.Add(deviceId, container);
            }
            await container.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Test mqtt client
        /// </summary>
        /// <returns></returns>
        private static async Task TestMqttClientAsync(CliOptions options) {

            // Get the gateway device id.
            var gatewayDeviceId = options.GetValue<string>("-d", "--deviceId");
            if (!kEdges.TryGetValue(gatewayDeviceId, out _)) {
                throw new ArgumentException("Gateway device not started.");
            }

            // Get mqtt client device id and create device to get connection string.
            var clientDeviceId = options.GetValueOrDefault("-c", "--clientId",
                Dns.GetHostName());
            var cs = await GetConnectionStringAsync(gatewayDeviceId, clientDeviceId,
                null).ConfigureAwait(false);

            var topic = options.GetValueOrDefault("-t", "--topic", "test_topic");

            var builder = new ContainerBuilder();
            builder.AddDiagnostics();
            builder.AddOptions();
            builder.RegisterModule<IoTEdgeHosting>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            // Connect to the IoT Edge device whose host name is always its deviceId
            Environment.SetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME", gatewayDeviceId);
            builder.Configure<IoTEdgeClientOptions>(o => {
                o.BypassCertVerification = options.IsProvidedOrNull("-a", "--acceptall")
                    ?? false;
                o.EdgeHubConnectionString = cs.ToString();
            });

            builder.Configure<IoTEdgeMqttOptions>(o => {
                o.QoS = options.GetValueOrDefault<byte?>("-q", "--qos", null);
                o.Retain = options.IsProvidedOrNull("-r", "--retain") ?? false;
                o.QueueSize = options.GetValueOrDefault<uint?>("-s", "--queuesize", null);
            });

            using var scope = builder.Build();

            var publisher = scope.Resolve<IEventPublisherClient>();
            var subscriber = scope.Resolve<IEventSubscriberClient>();
            var serializer = scope.Resolve<IJsonSerializer>();

            // Subscribe to topic
            var bufSize = options.GetValueOrDefault("-b", "--bufsize", 160 * 1024);
            var numberOfMessages = options.GetValueOrDefault("-c", "--count", 100);
            var buffer = new byte[bufSize];
            new Random().NextBytes(buffer);
            var count = 1;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using (await subscriber.SubscribeAsync(topic, (b, p) => {
                var m = serializer.Deserialize<dynamic>(b);
                var id = (int)m.i;
                var took = DateTime.UtcNow - (DateTime)m.start;
                Console.WriteLine($"{id} took {took.TotalMilliseconds} ms");
                if (++count >= numberOfMessages) {
                    // Done
                    tcs.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false)) {
                for (var i = 1; i <= numberOfMessages; i++) {
                    var msg = serializer.SerializeToBytes(new {
                        i,
                        start = DateTime.UtcNow,
                        buffer
                    }).ToArray();
                    await publisher.PublishAsync(topic, msg).ConfigureAwait(false);
                }
                await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Create mqtt client device
        /// </summary>
        /// <param name="gateway"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        private static async Task<ConnectionString> GetConnectionStringAsync(
            string gateway, string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var configuration = new ConfigurationBuilder()
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddFromKeyVault()
                .Build();
            var iotHubCs = configuration.GetValue<string>(
                PcsVariable.PCS_IOTHUB_CONNSTRING, null);
            if (string.IsNullOrEmpty(iotHubCs)) {
                iotHubCs = configuration.GetValue<string>("_HUB_CS", null);
            }
            if (string.IsNullOrEmpty(iotHubCs)) {
                throw new ArgumentException("Missing connection string.");
            }
            if (!ConnectionString.TryParse(iotHubCs, out var connectionString)) {
                throw new ArgumentException("Bad connection string.");
            }
            var config = connectionString.ToIoTHubOptions();
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            try {
                var registration = await registry.GetRegistrationAsync(
                    gateway, null, default).ConfigureAwait(false);
                // Register device with scope
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    ModuleId = moduleId,
                    DeviceScope = registration.DeviceScope,
                    Hub = registration.Hub
                }, false, default).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Gateway {deviceId} exists.", deviceId);
            }
            return await registry.GetConnectionStringAsync(deviceId).ConfigureAwait(false);
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintHelp() {
            Console.WriteLine(
                @"
Extension Tester
usage:       [operation] [options]

Operations (Mutually exclusive):
        start   Tests network scanning.
             -d, --deviceId  Device Id

        stop    Tests port scanning.
             -d, --deviceId  Device Id

        test     Tests mqtt client using an echo topic
             -d, --deviceId  Device Id
             -t, --topic     Topic to publish and subscribe
             -c, --count     Number of messages to send with - def: 100
             -b, --bufsize   Size of buffer to allocate - def: 160k
             -q, --qos       Quality of service (0, def: 1, 2)
             -a, --acceptall Accept all certificates
             -s, --queuesize Max send queue size
"
                );
        }
    }
}

