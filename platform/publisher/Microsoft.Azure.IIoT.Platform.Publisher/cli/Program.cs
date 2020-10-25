// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Module.Cli {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Sample;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
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
    using Microsoft.Extensions.Logging;
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
            var withServer = false;
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
Usage:       Microsoft.Azure.IIoT.Platform.Publisher.Module.Cli [options]

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

            var logger = ConsoleLogger.CreateLogger(LogLevel.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                logger.LogCritical(e.ExceptionObject as Exception, "Exception");
                Console.WriteLine(e);
            };

            try {
                if (!withServer) {
                    HostAsync(config, diagnostics, logger, deviceId,
                        moduleId, args, !checkTrust).Wait();
                }
                else {
                    WithServerAsync(config, diagnostics, logger, deviceId,
                        moduleId, args).Wait();
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
            ILogger logger, string deviceId, string moduleId, string[] args, bool acceptAll) {
            Console.WriteLine("Create or retrieve connection string...");

            var cs = await Retry.WithExponentialBackoff(logger,
                () => AddOrGetAsync(config, diagnostics, deviceId, moduleId)).ConfigureAwait(false);

            Console.WriteLine("Starting publisher module...");
            var arguments = args.ToList();
            arguments.Add($"--ec={cs}");
            arguments.Add($"--di=10");
            if (acceptAll) {
                arguments.Add("--aa");
            }
            Service.Program.Main(arguments.ToArray());
            Console.WriteLine("Publisher module exited.");
        }

        /// <summary>
        /// Setup publishing from sample server
        /// </summary>
        private static async Task WithServerAsync(IIoTHubConfig config, ILogAnalyticsConfig diagnostics,
            ILogger logger, string deviceId, string moduleId, string[] args) {
            var fileName = Path.GetRandomFileName() + ".json";
            try {
                using (var cts = new CancellationTokenSource())
                using (var server = new ServerWrapper(logger)) { // Start test server

                    var arguments = args.ToList();

                    // Start publisher module
                    var host = Task.Run(() => HostAsync(config, diagnostics, logger, deviceId,
                        moduleId, arguments.ToArray(), false), cts.Token);

                    Console.WriteLine("Press key to cancel...");
                    Console.ReadKey();

                    logger.LogInformation("Server exiting - tear down publisher...");
                    cts.Cancel();

                    await host.ConfigureAwait(false);
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
            var logger = ConsoleLogger.CreateLogger(LogLevel.Error);
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
                }, false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Gateway {deviceId} exists.", deviceId);
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
                }, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Module {moduleId} exists...", moduleId);
            }
            var cs = await registry.GetConnectionStringAsync(deviceId, moduleId).ConfigureAwait(false);
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
                _server = RunSampleServerAsync(logger, _cts.Token);
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
            private static async Task RunSampleServerAsync(ILogger logger, CancellationToken ct) {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                ct.Register(() => tcs.TrySetResult(true));
                using (var server = new ServerConsoleHost(new ServerFactory(logger) {
                    LogStatus = false
                }, logger) {
                    AutoAccept = true
                }) {
                    logger.LogInformation("Starting server.");
                    await server.StartAsync(new List<int> { 51210 }).ConfigureAwait(false);
                    logger.LogInformation("Server started.");
                    await tcs.Task.ConfigureAwait(false);
                    logger.LogInformation("Server exited.");
                }
            }

            private readonly CancellationTokenSource _cts;
            private readonly Task _server;
        }
    }
}