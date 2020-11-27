// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Cli {
    using Microsoft.Azure.IIoT.Platform.Discovery.Services;
    using Microsoft.Azure.IIoT.Platform.Discovery;
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Transport.Probe;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics.Runtime;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Net;
    using Microsoft.Azure.IIoT.Net.Models;
    using Microsoft.Azure.IIoT.Net.Scanner;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test client for opc ua services
    /// </summary>
    public static class Program {
        private enum Op {
            None,
            MakeSupervisor,
            ClearSupervisors,
            ClearRegistry,
            TestOpcUaDiscoveryService,
            TestOpcUaServerScanner,
            TestNetworkScanner,
            TestPortScanner
        }

        /// <summary>
        /// Test client entry point
        /// </summary>
        public static void Main(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }

            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Console.WriteLine("unhandled: " + e.ExceptionObject);
            var op = Op.None;
            string deviceId = null, moduleId = null;
            string addressRanges = null;
            var stress = false;
            var host = Utils.GetHostName();
            string cs = null;
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "-C":
                        case "--connection-string":
                            i++;
                            if (i < args.Length) {
                                cs = args[i];
                                break;
                            }
                            throw new ArgumentException(
                                "Missing arguments for connection string");
                        case "--make-supervisor":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.MakeSupervisor;
                            i++;
                            if (i < args.Length) {
                                deviceId = args[i];
                                i++;
                                if (i < args.Length) {
                                    moduleId = args[i];
                                    break;
                                }
                            }
                            throw new ArgumentException("Missing arguments to make iotedge device");
                        case "--clear-registry":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.ClearRegistry;
                            break;
                        case "--clear-supervisors":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.ClearSupervisors;
                            break;
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
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Test host
usage:       [options] operation [args]

Options:

     -C
    --connection-string     IoT Hub owner connection string to use to
                            connect to IoT hub to create edge identity.
                            If not provided, read from environment.
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
                        HostAsync(cs, deviceId, moduleId).Wait();
                        break;
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
        /// Host the module giving it its connection string.
        /// </summary>
        private static async Task HostAsync(string iotHubCs, string deviceId, string moduleId) {
            var configuration = new ConfigurationBuilder()
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddFromKeyVault()
                .Build();
            if (string.IsNullOrEmpty(iotHubCs)) {
                iotHubCs = configuration.GetValue<string>(PcsVariable.PCS_IOTHUB_CONNSTRING, null);
                if (string.IsNullOrEmpty(iotHubCs)) {
                    iotHubCs = configuration.GetValue<string>("_HUB_CS", null);
                }
                if (string.IsNullOrEmpty(iotHubCs)) {
                    throw new ArgumentException("Missing connection string.");
                }
            }
            if (!ConnectionString.TryParse(iotHubCs, out var connectionString)) {
                throw new ArgumentException("Bad connection string.");
            }
            var config = connectionString.ToIoTHubOptions();
            if (deviceId == null) {
                deviceId = Utils.GetHostName();
                Console.WriteLine($"Using <deviceId> '{deviceId}'");
            }
            if (moduleId == null) {
                moduleId = "registry";
                Console.WriteLine($"Using <moduleId> '{moduleId}'");
            }

            var diagnostics = new LogAnalyticsConfig(configuration).ToOptions().Value;

            Console.WriteLine("Create or retrieve connection string...");
            var logger = Log.Console(LogLevel.Error);
            var cs = await Retry.WithExponentialBackoff(logger,
                () => AddOrGetAsync(config, diagnostics, deviceId, moduleId)).ConfigureAwait(false);

            Console.WriteLine("Starting discovery service...");
            var arguments = new List<string> {
                $"EdgeHubConnectionString={cs}"
            };
            Service.Program.Main(arguments.ToArray());
            Console.WriteLine("Discovery service exited.");
        }

        /// <summary>
        /// Add or get module identity
        /// </summary>
        private static async Task<ConnectionString> AddOrGetAsync(IOptions<IoTHubOptions> config,
            LogAnalyticsOptions diagnostics, string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            try {
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    Tags = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Gateway
                    },
                    Capabilities = new DeviceCapabilitiesModel {
                        IotEdge = true
                    }
                }, false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Gateway {deviceId} exists.", deviceId);
            }
            try {
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    ModuleId = moduleId,
                    Properties = new TwinPropertiesModel {
                        Desired = new Dictionary<string, VariantValue> {
                            [nameof(diagnostics.LogWorkspaceId)] = diagnostics?.LogWorkspaceId,
                            [nameof(diagnostics.LogWorkspaceKey)] = diagnostics?.LogWorkspaceKey
                        }
                    }
                }, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Module {moduleId} exists...", moduleId);
            }
            var cs = await registry.GetConnectionStringAsync(deviceId, moduleId).ConfigureAwait(false);
            return cs;
        }

        /// <summary>
        /// Test port scanning
        /// </summary>
        private static async Task TestPortScannerAsync(string host, bool opc) {
            var logger = Log.Console();
            var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                var results = await scanning.ScanAsync(
                    PortRange.All.SelectMany(r => r.GetEndpoints(addresses.First())),
                    opc ? new ServerProbe(logger) : null, cts.Token).ConfigureAwait(false);
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
            var logger = Log.Console();
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10))) {
                var watch = Stopwatch.StartNew();
                var scanning = new ScanServices(logger);
                var results = await scanning.ScanAsync(NetworkClass.Wired, cts.Token).ConfigureAwait(false);
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
            using (var logger = StackLogger.Create(Log.Console()))
            using (var config = new TestClientServicesConfig())
            using (var client = new ClientServices(logger.Logger, config))
            using (var scanner = new DiscoveryServices(client,
                new NewtonSoftJsonSerializer(), new ConsoleListener(), logger.Logger)) {
                var rand = new Random();
                while (true) {
                    var configuration = new DiscoveryConfigModel {
                        IdleTimeBetweenScans = TimeSpan.FromMilliseconds(1),
                        AddressRangesToScan = addressRanges
                    };
                    await scanner.ConfigureAsync(DiscoveryMode.Scan, configuration).ConfigureAwait(false);
                    await scanner.ScanAsync().ConfigureAwait(false);
                    await Task.Delay(!stress ? TimeSpan.FromMinutes(10) :
                        TimeSpan.FromMilliseconds(rand.Next(0, 120000))).ConfigureAwait(false);
                    logger.Logger.LogInformation("Stopping discovery!");
                    await scanner.ConfigureAsync(DiscoveryMode.Off, null).ConfigureAwait(false);
                    await scanner.ScanAsync().ConfigureAwait(false);
                    if (!stress) {
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        private class ConsoleListener : IApplicationRegistryListener,
            IEndpointRegistryListener, IDiscoveryResultHandler, ITwinRegistryListener {

            /// <inheritdoc/>
            public Task ReportResultsAsync(IEnumerable<DiscoveryResultModel> results,
                CancellationToken ct) {
                Console.WriteLine(_serializer.SerializePretty(results));
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationDeletedAsync(OperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Deleted {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationNewAsync(OperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Created {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationLostAsync(OperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Lost {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationFoundAsync(OperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Found {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnApplicationUpdatedAsync(OperationContextModel context,
                ApplicationInfoModel application) {
                Console.WriteLine($"Updated {application.ApplicationId}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointDeletedAsync(OperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Deleted {endpoint.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointNewAsync(OperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Created {endpoint.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointLostAsync(OperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Lost {endpoint.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointFoundAsync(OperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Found {endpoint.Id}");
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task OnEndpointUpdatedAsync(OperationContextModel context,
                EndpointInfoModel endpoint) {
                Console.WriteLine($"Updated {endpoint.Id}");
                return Task.CompletedTask;
            }

            public Task OnTwinActivatedAsync(OperationContextModel context,
                TwinInfoModel twin) {
                Console.WriteLine($"Activated {twin.Id}");
                return Task.CompletedTask;
            }

            public Task OnTwinUpdatedAsync(OperationContextModel context,
                TwinInfoModel twin) {
                Console.WriteLine($"Updated {twin.Id}");
                return Task.CompletedTask;
            }

            public Task OnTwinDeactivatedAsync(OperationContextModel context,
                TwinInfoModel twin) {
                Console.WriteLine($"Deactivated {twin.Id}");
                return Task.CompletedTask;
            }

            private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
        }
    }
}
