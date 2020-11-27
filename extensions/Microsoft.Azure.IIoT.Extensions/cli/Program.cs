// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Core.Cli {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Extensions.Mqtt;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Net;
    using Microsoft.Azure.IIoT.Net.Models;
    using Microsoft.Azure.IIoT.Net.Scanner;
    using Microsoft.Azure.IIoT.Utils;
    using Autofac;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.Serializers;

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
                            Console.WriteLine("Extensions Tester");
                            interactive = true;
                            break;
                        case "ports":
                            options = new CliOptions(args);
                            await TestPortScannerAsync(options).ConfigureAwait(false);
                            break;
                        case "network":
                            options = new CliOptions(args);
                            await TestNetworkScannerAsync(options).ConfigureAwait(false);
                            break;
                        case "mqtt":
                            options = new CliOptions(args);
                            await TestMqttClientAsync(options).ConfigureAwait(false);
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

        /// <summary>
        /// Test port scanning
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private static async Task TestPortScannerAsync(CliOptions options) {
            var host = options.GetValue<string>("-H", "--host");
            var logger = Log.Console();
            var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            using (var cts = new CancellationTokenSource(
                options.GetValueOrDefault("-d", "--duration", TimeSpan.FromMinutes(10)))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                DumpMemory();
                var results = await scanning.ScanAsync(
                    PortRange.All.SelectMany(r => r.GetEndpoints(addresses.First())),
                    cts.Token).ConfigureAwait(false);
                foreach (var result in results) {
                    Console.WriteLine($"Found {result} open.");
                }
                Console.WriteLine($"Scan took: {watch.Elapsed}");
                DumpMemory();
            }
        }

        /// <summary>
        /// Test network scanning
        /// </summary>
        /// <returns></returns>
        private static async Task TestNetworkScannerAsync(CliOptions options) {
            var logger = Log.Console();
            using (var cts = new CancellationTokenSource(
                options.GetValueOrDefault("-d", "--duration", TimeSpan.FromMinutes(10)))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                DumpMemory();
                var results = await scanning.ScanAsync(NetworkClass.Wired, cts.Token).ConfigureAwait(false);
                foreach (var result in results) {
                    Console.WriteLine($"Found {result.Address}...");
                }
                Console.WriteLine($"Scan took: {watch.Elapsed}");
                DumpMemory();
            }
        }

        /// <summary>
        /// Test mqtt client
        /// </summary>
        /// <returns></returns>
        private static async Task TestMqttClientAsync(CliOptions options) {
            var topic = options.GetValueOrDefault("-t", "--topic", "test_topic");

            var builder = new ContainerBuilder();
            builder.AddDiagnostics();
            builder.AddOptions();
            builder.RegisterModule<MqttClientModule>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            builder.Configure<MqttOptions>(o => {
                o.ClientId = options.GetValueOrDefault<string>("-i", "--id", null);
                o.HostName = options.GetValueOrDefault<string>("-H", "--host", null);
                o.Port = options.GetValueOrDefault<int?>("-P", "--port", null);
                o.QoS = options.GetValueOrDefault<byte?>("-q", "--qos", null);
                o.UserName = options.GetValueOrDefault<string>("-u", "--user", null)?.TrimQuotes();
                o.Password = options.GetValueOrDefault<string>("-p", "--password", null)?.TrimQuotes();
                o.AllowUntrustedCertificates = options.IsProvidedOrNull("-a", "--acceptall");
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
        /// Write memory dump
        /// </summary>
        private static void DumpMemory() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"GC Mem: {GC.GetTotalMemory(false) / 1024} kb, Working set /" +
              $" Private Mem: {Process.GetCurrentProcess().WorkingSet64 / 1024} kb / " +
              $"{Process.GetCurrentProcess().PrivateMemorySize64 / 1024} kb, Handles:" +
              Process.GetCurrentProcess().HandleCount);
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
    network  Tests network scanning.
         -d, --duration  Scan timeout

    ports    Tests port scanning.
         -H, --host      Host that should be scanned
         -d, --duration  Scan timeout

    mqtt     Tests mqtt client using an echo topic
         -t, --topic     Topic to publish and subscribe
         -c, --count     Number of messages to send with - def: 100
         -b, --bufsize   Size of buffer to allocate - def: 160k
         -H, --host      Host name of mqtt broker - def: localhost
         -P, --port      Port to connect on - def: 8883.
         -q, --qos       Quality of service (0, def: 1, 2)
         -u, --user      Optional User name to use
         -p, --password  Optional Password to use
         -a, --acceptall Accept all certificates
         -s, --queuesize Max send queue size
"
                );
        }
    }
}

