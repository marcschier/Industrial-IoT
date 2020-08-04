// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Cli {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Sample;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Services;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Configuration;
    using Opc.Ua;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;

    /// <summary>
    /// Publisher module host process
    /// </summary>
    public class Program {

        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args) {
            var checkTrust = true;
            var legacyTest = false;
            var withServer = false;
            var verbose = false;
            var scale = 1;
            string deviceId = null, moduleId = null;
            Console.WriteLine("Publisher module command line interface.");
            var configuration = new ConfigurationBuilder()
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(EnvironmentVariableTarget.User)
                // Above configuration providers will provide connection
                // details for KeyVault configuration provider.
                .AddFromKeyVault(providerPriority: ConfigurationProviderPriority.Lowest)
                .Build();
            var cs = configuration.GetValue<string>(PcsVariable.PCS_IOTHUB_CONNSTRING, null);
            if (string.IsNullOrEmpty(cs)) {
                cs = configuration.GetValue<string>("_HUB_CS", null);
            }
            var diagnostics = new LogAnalyticsConfig(configuration);
            IIoTHubConfig config = null;
            var unknownArgs = new List<string>();
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
                        case "-?":
                        case "-h":
                        case "--help":
                            throw new ArgumentException("Help");
                        case "-t":
                        case "--only-trusted":
                            checkTrust = true;
                            break;
                        case "-s":
                        case "--with-server":
                            withServer = true;
                            break;
                        case "-l":
                        case "--legacy-test":
                        case "--scale-test":
                            legacyTest = true;
                            withServer = true;
                            i++;
                            if (i < args.Length) {
                                if (!int.TryParse(args[i], out scale)) {
                                    i--;
                                }
                            }
                            break;
                        case "-v":
                        case "--verbose":
                            verbose = true;
                            break;
                        default:
                            unknownArgs.Add(args[i]);
                            break;
                    }
                }
                if (string.IsNullOrEmpty(cs)) {
                    throw new ArgumentException("Missing connection string.");
                }
                if (!ConnectionString.TryParse(cs, out var connectionString)) {
                    throw new ArgumentException("Bad connection string.");
                }
                config = connectionString.ToIoTHubConfig();

                if (deviceId == null) {
                    deviceId = Utils.GetHostName();
                    Console.WriteLine($"Using <deviceId> '{deviceId}'");
                }
                if (moduleId == null) {
                    moduleId = "publisher";
                    Console.WriteLine($"Using <moduleId> '{moduleId}'");
                }

                args = unknownArgs.ToArray();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Usage:       Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Cli [options]

Options:
     -C
    --connection-string
             IoT Hub owner connection string to use to connect to IoT hub for
             operations on the registry.  If not provided, read from environment.

     -l
    --legacy-test
    --scale-test <scale-count>
            Spins up a test server and subscribes to clock on server
            <scale-count> times.

    --help
     -?
     -h      Prints out this help.
"
                    );
                return;
            }

            var logger = ConsoleLogger.Create(LogEventLevel.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                logger.Fatal(e.ExceptionObject as Exception, "Exception");
                Console.WriteLine(e);
            };

            try {
                if (!withServer) {
                    HostAsync(config, diagnostics, logger, deviceId,
                        moduleId, args, verbose, !checkTrust).Wait();
                }
                else {
                    WithServerAsync(config, diagnostics, logger, deviceId,
                        moduleId, args, legacyTest, scale, verbose).Wait();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Host the module giving it its connection string.
        /// </summary>
        private static async Task HostAsync(IIoTHubConfig config, ILogAnalyticsConfig diagnostics,
            ILogger logger, string deviceId, string moduleId, string[] args, bool verbose, bool acceptAll) {
            Console.WriteLine("Create or retrieve connection string...");

            var cs = await Retry.WithExponentialBackoff(logger,
                () => AddOrGetAsync(config, diagnostics, deviceId, moduleId));

            // Hook event source
            using (var broker = new EventSourceBroker()) {
                LogControl.Level.MinimumLevel = verbose ?
                    LogEventLevel.Verbose : LogEventLevel.Information;

                Console.WriteLine("Starting publisher module...");
                broker.Subscribe(IoTSdkLogger.EventSource, new IoTSdkLogger(logger));
                var arguments = args.ToList();
                arguments.Add($"--ec={cs}");
                arguments.Add($"--di=10");
                if (acceptAll) {
                    arguments.Add("--aa");
                }
                Module.Program.Main(arguments.ToArray());
                Console.WriteLine("Publisher module exited.");
            }
        }

        /// <summary>
        /// setup publishing from sample server
        /// </summary>
        private static async Task WithServerAsync(IIoTHubConfig config, ILogAnalyticsConfig diagnostics,
            ILogger logger, string deviceId, string moduleId, string[] args, bool startLegacy, int scale,
            bool verbose) {
            var fileName = Path.GetRandomFileName() + ".json";
            try {
                using (var cts = new CancellationTokenSource())
                using (var server = new ServerWrapper(logger)) { // Start test server

                    var arguments = args.ToList();
                    if (startLegacy) {
                        // Write publishing
                        var pf = new PublishedNodesFile(fileName, new NewtonSoftJsonSerializer(), logger);
                        pf.Write(new WriterGroupModel {
                            DataSetWriters = new List<DataSetWriterModel> {
                                new DataSetWriterModel {
                                    DataSet = new PublishedDataSetModel {
                                        DataSetSource = new PublishedDataSetSourceModel {
                                            Connection = new ConnectionModel {
                                                Endpoint = new EndpointModel {
                                                    Url = server.EndpointUrl
                                                }
                                            },
                                            PublishedVariables = new PublishedDataItemsModel {
                                                PublishedData = new List<PublishedDataSetVariableModel> {
                                                    new PublishedDataSetVariableModel {
                                                        PublishedVariableNodeId = "i=2258"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        });
                        arguments.Add($"--pf={fileName}");
                        if (scale > 1) {
                            arguments.Add($"--sc={scale}");
                        }
                    }

                    // Start publisher module
                    var host = Task.Run(() => HostAsync(config, diagnostics, logger, deviceId,
                        moduleId, arguments.ToArray(), verbose, false), cts.Token);

                    Console.WriteLine("Press key to cancel...");
                    Console.ReadKey();

                    logger.Information("Server exiting - tear down publisher...");
                    cts.Cancel();

                    await host;
                }
            }
            catch (OperationCanceledException) { }
            finally {
                Try.Op(() => File.Delete(fileName));
            }
        }

        /// <summary>
        /// Add or get module identity
        /// </summary>
        private static async Task<ConnectionString> AddOrGetAsync(IIoTHubConfig config,
            ILogAnalyticsConfig diagnostics, string deviceId, string moduleId) {
            var logger = ConsoleLogger.Create(LogEventLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            try {
                await registry.CreateOrUpdateAsync(new DeviceTwinModel {
                    Id = deviceId,
                    Tags = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Gateway
                    },
                    Capabilities = new DeviceCapabilitiesModel {
                        IotEdge = true
                    }
                }, false, CancellationToken.None);
            }
            catch (ConflictingResourceException) {
                logger.Information("Gateway {deviceId} exists.", deviceId);
            }
            try {
                await registry.CreateOrUpdateAsync(new DeviceTwinModel {
                    Id = deviceId,
                    ModuleId = moduleId,
                    Properties = new TwinPropertiesModel {
                        Desired = new Dictionary<string, VariantValue> {
                            [nameof(diagnostics.LogWorkspaceId)] = diagnostics?.LogWorkspaceId,
                            [nameof(diagnostics.LogWorkspaceKey)] = diagnostics?.LogWorkspaceKey
                        }
                    }
                }, true, CancellationToken.None);
            }
            catch (ConflictingResourceException) {
                logger.Information("Module {moduleId} exists...", moduleId);
            }
            var cs = await registry.GetConnectionStringAsync(deviceId, moduleId);
            return cs;
        }

        /// <summary>
        /// Wraps server and disposes after use
        /// </summary>
        private class ServerWrapper : IDisposable {

            public string EndpointUrl { get; }

            /// <summary>
            /// Create wrapper
            /// </summary>
            public ServerWrapper(ILogger logger) {
                _cts = new CancellationTokenSource();
                _server = RunSampleServerAsync(_cts.Token, logger);
                EndpointUrl = "opc.tcp://" + Utils.GetHostName() +
                    ":51210/UA/SampleServer";
            }

            /// <inheritdoc/>
            public void Dispose() {
                _cts.Cancel();
                _server.Wait();
                _cts.Dispose();
            }

            /// <summary>
            /// Run server until cancelled
            /// </summary>
            private static async Task RunSampleServerAsync(CancellationToken ct, ILogger logger) {
                var tcs = new TaskCompletionSource<bool>();
                ct.Register(() => tcs.TrySetResult(true));
                using (var server = new ServerConsoleHost(new ServerFactory(logger) {
                    LogStatus = false
                }, logger) {
                    AutoAccept = true
                }) {
                    logger.Information("Starting server.");
                    await server.StartAsync(new List<int> { 51210 });
                    logger.Information("Server started.");
                    await tcs.Task;
                    logger.Information("Server exited.");
                }
            }

            private readonly CancellationTokenSource _cts;
            private readonly Task _server;
        }

        /// <summary>
        /// Sdk logger event source hook
        /// </summary>
        sealed class IoTSdkLogger : EventSourceSerilogSink {
            public IoTSdkLogger(ILogger logger) :
                base(logger.ForContext("SourceContext", EventSource.Replace('-', '.'))) {
            }

            public override void OnEvent(EventWrittenEventArgs eventData) {
                switch (eventData.EventName) {
                    case "Enter":
                    case "Exit":
                    case "Associate":
                        WriteEvent(LogEventLevel.Verbose, eventData);
                        break;
                    default:
                        WriteEvent(LogEventLevel.Debug, eventData);
                        break;
                }
            }

            // ddbee999-a79e-5050-ea3c-6d1a8a7bafdd
            public const string EventSource = "Microsoft-Azure-Devices-Device-Client";
        }
    }
}
