// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Edge.Module {
    using Microsoft.Azure.IIoT.Platform.Registry.Edge.Module.Runtime;
    using Microsoft.Azure.IIoT.Platform.Registry.Edge.Module.Controllers;
    using Microsoft.Azure.IIoT.Platform.Registry.Edge.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Extensions.Configuration;
    using Autofac;
    using System;
    using System.Runtime.Loader;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Threading;
    using Serilog;
    using Prometheus;

    /// <summary>
    /// Module Process
    /// </summary>
    public class ModuleProcess : IProcessControl {

        /// <summary>
        /// Whether the module is running
        /// </summary>

        public event EventHandler<bool> OnRunning;

        /// <summary>
        /// Create process
        /// </summary>
        /// <param name="config"></param>
        public ModuleProcess(IConfiguration config) {
            _config = config;
            _exitCode = 0;
            _exit = new TaskCompletionSource<bool>();
            AssemblyLoadContext.Default.Unloading += _ => _exit.TrySetResult(true);
        }

        /// <inheritdoc/>
        public void Reset() {
            _reset.TrySetResult(true);
        }

        /// <inheritdoc/>
        public void Exit(int exitCode) {

            // Shut down gracefully.
            _exitCode = exitCode;
            _exit.TrySetResult(true);

            if (Host.IsContainer) {
                // Set timer to kill the entire process after 5 minutes.
                var _ = new Timer(o => {
                    Log.Logger.Fatal("Killing non responsive module process!");
                    Process.GetCurrentProcess().Kill();
                }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            }
        }

        /// <summary>
        /// Run module host
        /// </summary>
        public async Task<int> RunAsync() {
            // Wait until the module unloads
            while (true) {
                using (var hostScope = ConfigureContainer(_config)) {
                    _reset = new TaskCompletionSource<bool>();
                    var module = hostScope.Resolve<IModuleHost>();
                    var identity = hostScope.Resolve<IIdentity>();
                    var client = hostScope.Resolve<IClientHost>();
                    var logger = hostScope.Resolve<ILogger>();
                    var config = new Config(_config);
                    IMetricServer server = null;
                    try {
                        // Start module
                        var version = GetType().Assembly.GetReleaseVersion().ToString();
                        logger.Information("Starting module OpcDiscovery version {version}.", version);
                        await module.StartAsync(IdentityType.Discoverer, version).ConfigureAwait(false);
                        await client.InitializeAsync().ConfigureAwait(false);
                        kDiscoveryModuleStart.WithLabels(
                            identity.DeviceId ?? "", identity.ModuleId ?? "").Inc();
                        if (hostScope.TryResolve(out server)) {
                            server.Start();
                        }
                        OnRunning?.Invoke(this, true);
                        await Task.WhenAny(_reset.Task, _exit.Task).ConfigureAwait(false);
                        if (_exit.Task.IsCompleted) {
                            logger.Information("Module exits...");
                            return _exitCode;
                        }
                        _reset = new TaskCompletionSource<bool>();
                        logger.Information("Module reset...");
                    }
                    catch (Exception ex) {
                        logger.Error(ex, "Error during module execution - restarting!");
                    }
                    finally {
                        if (server != null) {
                            await server.StopAsync().ConfigureAwait(false);
                        }
                        await module.StopAsync().ConfigureAwait(false);
                        kDiscoveryModuleStart.WithLabels(
                            identity.DeviceId ?? "", identity.ModuleId ?? "").Set(0);
                        OnRunning?.Invoke(this, false);
                    }
                }
            }
        }

        /// <summary>
        /// Autofac configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private IContainer ConfigureContainer(IConfiguration configuration) {

            var config = new Config(configuration);
            var builder = new ContainerBuilder();

            // Register configuration interfaces
            builder.RegisterInstance(config)
                .AsImplementedInterfaces();
            builder.RegisterInstance(this)
                .AsImplementedInterfaces();

            // register logger
            builder.AddDebugDiagnostics(config);

            // Register module framework
            builder.RegisterModule<ModuleFramework>();
            builder.RegisterModule<IoTEdgeHosting>();
            builder.RegisterModule<LogAnalyticsMetrics>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            // Register opc ua services
            builder.RegisterType<ClientServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<StackLogger>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .AutoActivate();

            // Register discovery services
            builder.RegisterType<DiscoveryServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ProgressPublisher>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces();

            // Register controllers
            builder.RegisterType<DiscoveryMethodsController>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DiagnosticSettingsController>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DiscoverySettingsController>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            return builder.Build();
        }

        private readonly IConfiguration _config;
        private readonly TaskCompletionSource<bool> _exit;
        private TaskCompletionSource<bool> _reset;
        private int _exitCode;
        private static readonly Gauge kDiscoveryModuleStart = Metrics
            .CreateGauge("iiot_edge_discovery_module_start", "discovery module started",
                new GaugeConfiguration {
                    LabelNames = new[] { "deviceid", "module" }
                });
    }
}
