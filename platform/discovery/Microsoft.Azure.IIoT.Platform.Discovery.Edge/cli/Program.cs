// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Edge.Cli {
    using Microsoft.Azure.IIoT.Platform.Discovery.Edge.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Transport.Probe;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Net;
    using Microsoft.Azure.IIoT.Net.Models;
    using Microsoft.Azure.IIoT.Net.Scanner;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Newtonsoft.Json;
    using Opc.Ua;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test client for opc ua services
    /// </summary>
    public class Program {
        private enum Op {
            None,
            TestOpcUaDiscoveryService,
            TestOpcUaServerScanner,
            TestNetworkScanner,
            TestPortScanner
        }

        /// <summary>
        /// Test client entry point
        /// </summary>
        public static void Main(string[] args) {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Console.WriteLine("unhandled: " + e.ExceptionObject);
            var op = Op.None;
            string addressRanges = null;
            var stress = false;
            var host = Utils.GetHostName();
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "--stress":
                            stress = true;
                            break;
                        case "--scan-ports":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestPortScanner;
                            i++;
                            if (i < args.Length) {
                                host = args[i];
                            }
                            break;
                        case "--scan-servers":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaServerScanner;
                            i++;
                            if (i < args.Length) {
                                host = args[i];
                            }
                            break;
                        case "--scan-net":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestNetworkScanner;
                            break;
                        case "--test-discovery":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaDiscoveryService;
                            i++;
                            if (i < args.Length) {
                                addressRanges = args[i];
                            }
                            break;
                        case "-?":
                        case "-h":
                        case "--help":
                            throw new ArgumentException("Help");
                        default:
                            throw new ArgumentException($"Unknown {args[i]}");
                    }
                }
                if (op == Op.None) {
                    throw new ArgumentException("Missing operation.");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Test discovery
usage:       [options] operation [args]

Options:

    --stress                Run test as stress test (if supported)
    --port / -p             Port to listen on
    --help / -? / -h        Prints out this help.

Operations (Mutually exclusive):

    --scan-net              Tests network scanning.
    --scan-ports            Tests port scanning.
    --scan-servers          Tests opc server scanning on single machine.
    --test-discovery        Tests discovery stuff.

"
                    );
                return;
            }

            try {
                Console.WriteLine($"Running {op}...");
                switch (op) {
                    case Op.TestNetworkScanner:
                        TestNetworkScannerAsync().Wait();
                        break;
                    case Op.TestPortScanner:
                        TestPortScannerAsync(host, false).Wait();
                        break;
                    case Op.TestOpcUaServerScanner:
                        TestPortScannerAsync(host, true).Wait();
                        break;
                    case Op.TestOpcUaDiscoveryService:
                        TestOpcUaDiscoveryServiceAsync(addressRanges, stress).Wait();
                        break;
                    default:
                        throw new ArgumentException("Unknown.");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return;
            }

            Console.WriteLine("Press key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Test port scanning
        /// </summary>
        private static async Task TestPortScannerAsync(string host, bool opc) {
            var logger = ConsoleOutLogger.Create();
            var addresses = await Dns.GetHostAddressesAsync(host);
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                var results = await scanning.ScanAsync(
                    PortRange.All.SelectMany(r => r.GetEndpoints(addresses.First())),
                    opc ? new ServerProbe(logger) : null, cts.Token);
                foreach (var result in results) {
                    Console.WriteLine($"Found {result} open.");
                }
                Console.WriteLine($"Scan took: {watch.Elapsed}");
            }
        }

        /// <summary>
        /// Test network scanning
        /// </summary>
        private static async Task TestNetworkScannerAsync() {
            var logger = ConsoleOutLogger.Create();
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                var results = await scanning.ScanAsync(NetworkClass.Wired, cts.Token);
                foreach (var result in results) {
                    Console.WriteLine($"Found {result.Address}...");
                }
                Console.WriteLine($"Scan took: {watch.Elapsed}");
            }
        }

        /// <summary>
        /// Test discovery
        /// </summary>
        private static async Task TestOpcUaDiscoveryServiceAsync(string addressRanges,
            bool stress) {
            using (var logger = StackLogger.Create(ConsoleLogger.Create()))
            using (var client = new ClientServices(logger.Logger, new TestClientServicesConfig()))
            using (var scanner = new DiscoveryServices(client, new ConsoleEmitter(),
                new NewtonSoftJsonSerializer(), logger.Logger)) {
                var rand = new Random();
                while (true) {
                    var configuration = new DiscoveryConfigModel {
                        IdleTimeBetweenScans = TimeSpan.FromMilliseconds(1),
                        AddressRangesToScan = addressRanges
                    };
                    await scanner.ConfigureAsync(DiscoveryMode.Scan, configuration);
                    await scanner.ScanAsync();
                    await Task.Delay(!stress ? TimeSpan.FromMinutes(10) :
                        TimeSpan.FromMilliseconds(rand.Next(0, 120000)));
                    logger.Logger.Information("Stopping discovery!");
                    await scanner.ConfigureAsync(DiscoveryMode.Off, null);
                    await scanner.ScanAsync();
                    if (!stress) {
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        private class ConsoleEmitter : ISettingsReporter, IEventClient, IIdentity {

            /// <inheritdoc/>
            public string Gateway => Utils.GetHostName();

            /// <inheritdoc/>
            public string DeviceId => Gateway;

            /// <inheritdoc/>
            public string ModuleId { get; } = "";

            /// <inheritdoc/>
            public Task SendEventAsync(string target, byte[] data, string contentType,
                string eventSchema, string contentEncoding, CancellationToken ct) {
                var json = Encoding.UTF8.GetString(data);
                var o = JsonConvert.DeserializeObject(json);
                Console.WriteLine(contentType);
                Console.WriteLine(_serializer.SerializePretty(o));
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public async Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
                string eventSchema, string contentEncoding, CancellationToken ct) {
                foreach (var data in batch) {
                    await SendEventAsync(target, data, contentType, contentType, contentEncoding, ct);
                }
            }

            /// <inheritdoc/>
            public Task ReportAsync(string propertyId, VariantValue value, CancellationToken ct) {
                Console.WriteLine($"{propertyId}={value}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task ReportAsync(IEnumerable<KeyValuePair<string, VariantValue>> properties,
                CancellationToken ct) {
                foreach (var prop in properties) {
                    Console.WriteLine($"{prop.Key}={prop.Value}");
                }
                return Task.CompletedTask;
            }

            private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
        }

        /// <inheritdoc/>
        private class ConsoleListener : IApplicationRegistryListener,
            IEndpointRegistryListener {

            /// <inheritdoc/>
            public Task OnApplicationDeletedAsync(RegistryOperationContextModel context,
                string applicationId, ApplicationInfoModel application) {
                Console.WriteLine($"Deleted {applicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationDisabledAsync(RegistryOperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Disabled {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationEnabledAsync(RegistryOperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Enabled {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationNewAsync(RegistryOperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Created {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationUpdatedAsync(RegistryOperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Updated {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointActivatedAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Activated {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointDeactivatedAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Deactivated {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointDeletedAsync(RegistryOperationContextModel context,
                string endpointId, EndpointInfoModel endpoint) {
                Console.WriteLine($"Deleted {endpointId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointDisabledAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Disabled {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointEnabledAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Enabled {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointNewAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Created {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointUpdatedAsync(RegistryOperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Updated {endpoint.Registration.Id}");
                return Task.CompletedTask;
            }
        }
    }
}
