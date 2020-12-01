// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Testing.Cli {
    using Microsoft.Azure.IIoT.Azure.IoTEdge.Testing;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.IO;

    /// <summary>
    /// Azure testing command line interface
    /// </summary>
    public sealed class Program : IDisposable {

        /// <summary>
        /// Configure Dependency injection
        /// </summary>
        public static IContainer ConfigureContainer(IConfiguration configuration) {
            var builder = new ContainerBuilder();

            builder.AddConfiguration(configuration);
            builder.AddOptions();
            builder.AddDiagnostics(builder => builder.AddDebug());
            builder.RegisterModule<NewtonSoftJsonModule>();
            builder.RegisterModule<IoTHubSupportModule>();
            builder.RegisterModule<IoTEdgeDeploymentModule>();

            // Create automatic deployment on startup
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            return builder.Build();
        }


        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args) {

            // Load hosting configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddFromKeyVault(allowInteractiveLogon: true)
                .Build();

            using (var scope = new Program(config)) {
                scope.RunAsync(args).Wait();
            }
        }


        /// <summary>
        /// Configure Dependency injection
        /// </summary>
        public Program(IConfiguration configuration) {
            var container = ConfigureContainer(configuration);
            _iotHubScope = container.BeginLifetimeScope();
        }

        /// <inheritdoc/>
        public void Dispose() {
            _iotHubScope.Dispose();
        }

        /// <summary>
        /// Run cli
        /// </summary>
        public async Task RunAsync(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }

            try {
                do {
                    if (_interactive) {
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
                                _interactive = false;
                                break;
                            case "console":
                                Console.WriteLine("Azure Tester");
                                _interactive = true;
                                break;
                            case "iotedge":
                                if (args.Length < 2) {
                                    throw new ArgumentException("Need a command!");
                                }
                                command = args[1];
                                options = new CliOptions(args, 2);
                                switch (command) {
                                    case "test":
                                        await RunAsync(TestMqttClientAsync, options).ConfigureAwait(false);
                                        break;
                                    case "start":
                                        await RunAsync(StartIoTEdgeAsync, options).ConfigureAwait(false);
                                        break;
                                    case "stop":
                                        await RunAsync(StopIoTEdgeAsync, options).ConfigureAwait(false);
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
                        if (!_interactive) {
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
                while (_interactive);
            }
            finally {
                await DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Stop an IoT edge
        /// </summary>
        private async Task StopIoTEdgeAsync(CliOptions options) {
            IoTEdgeDevice container = null;
            lock (_edges) {
                var deviceId = options.GetValue<string>("-d", "--deviceId");
                if (!_edges.TryGetValue(deviceId, out container)) {
                    return;
                }
                _edges.Remove(deviceId);
            }
            await container.StopAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Start an IoT edge
        /// </summary>
        private async Task StartIoTEdgeAsync(CliOptions options) {
            IoTEdgeDevice container = null;
            lock (_edges) {
                var deviceId = options.GetValue<string>("-d", "--deviceId");
                if (_edges.ContainsKey(deviceId)) {
                    return;
                }
                var registry = _iotHubScope.Resolve<IDeviceTwinServices>();
                var logger = _iotHubScope.Resolve<ILogger<IoTEdgeDevice>>();
                container = new IoTEdgeDevice(registry, deviceId, logger);
                _edges.Add(deviceId, container);
            }
            await container.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Test mqtt client
        /// </summary>
        private async Task TestMqttClientAsync(CliOptions options) {

            var logger = _iotHubScope.Resolve<ILogger>();
            // Get the gateway device id.
            var gatewayDeviceId = options.GetValue<string>("-d", "--deviceId");
            if (!_edges.TryGetValue(gatewayDeviceId, out _)) {
                throw new ArgumentException("Gateway device not started.");
            }

            // Get mqtt client device id and create device to get connection string.
            var clientDeviceId = options.GetValueOrDefault("-c", "--clientId",
                Dns.GetHostName());
            var registry = _iotHubScope.Resolve<IDeviceTwinServices>();
            try {
                var registration = await registry.GetRegistrationAsync(
                    gatewayDeviceId, null, default).ConfigureAwait(false);
                // Register device with scope
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = clientDeviceId,
                    ModuleId = null,
                    DeviceScope = registration.DeviceScope,
                    Hub = registration.Hub
                }, false, default).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Client {deviceId} exists.", clientDeviceId);
            }
            var cs = await registry.GetConnectionStringAsync(clientDeviceId).ConfigureAwait(false);

            // Build the edge client container.
            var builder = new ContainerBuilder();
            builder.AddDiagnostics();
            builder.AddOptions();
            builder.RegisterModule<HttpClientModule>();
            builder.RegisterModule<IoTEdgeHosting>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            // Connect to the IoT Edge device whose host name is always its deviceId
            Environment.SetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME",
                options.IsProvidedOrNull("-l", "--localhost") != null ? "localhost" : gatewayDeviceId);
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
            var topic = options.GetValueOrDefault("-t", "--topic", "test_topic");
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
        /// Run interruptable command
        /// </summary>
        /// <param name="options"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task RunAsync(Func<CliOptions, Task> command, CliOptions options) {
            if (_interactive) {
                var c = Console.TreatControlCAsInput;
                Console.TreatControlCAsInput = true;
                try {
                    await Task.WhenAny(command(options),
                        Task.Run(() => Console.ReadKey())).ConfigureAwait(false);
                }
                finally {
                    Console.TreatControlCAsInput = c;
                }
            }
            else {
                await command(options).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Stop an IoT edge
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task DisposeAsync() {
            List<IoTEdgeDevice> edges;
            lock (_edges) {
                edges = _edges.Values.ToList();
                _edges.Clear();
            }
            foreach (var edge in edges) {
                await edge.StopAsync().ConfigureAwait(false);
            }
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

        private readonly Dictionary<string, IoTEdgeDevice> _edges = new();
        private readonly ILifetimeScope _iotHubScope;
        private bool _interactive;
    }
}

