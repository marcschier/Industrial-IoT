// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module {
    using Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Runtime;
    using Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Controllers;
    using Microsoft.Azure.IIoT.Platform.Twin.Edge;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
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
        /// Whethr the module is running
        /// </summary>

        public event EventHandler<bool> OnRunning;

        /// <summary>
        /// Create process
        /// </summary>
        /// <param name="config"></param>
        /// <param name="injector"></param>
        public ModuleProcess(IConfiguration config, IInjector injector = null) {
            _config = config;
            _injector = injector;
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
#pragma warning disable IDE0067 // Dispose objects before losing scope
                var _ = new Timer(o => {
                    Log.Logger.Fatal("Killing non responsive module process!");
                    Process.GetCurrentProcess().Kill();
                }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
#pragma warning restore IDE0067 // Dispose objects before losing scope
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
                        var version = GetType().Assembly.GetReleaseVersion().ToString();
                        logger.Information("Starting module OpcTwin version {version}.",
                            version);
                        await module.StartAsync(IdentityType.Supervisor, version);
                        if (hostScope.TryResolve(out server)) {
                            server.Start();
                        }
                        kTwinModuleStart.WithLabels(
                            identity.DeviceId ?? "", identity.ModuleId ?? "").Inc();
                        await client.InitializeAsync();
                        OnRunning?.Invoke(this, true);
                        await Task.WhenAny(_reset.Task, _exit.Task);
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
                        kTwinModuleStart.WithLabels(
                            identity.DeviceId ?? "", identity.ModuleId ?? "").Set(0);
                        if (server != null) {
                            await server.StopAsync();
                        }
                        await module.StopAsync();
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
            builder.RegisterInstance(config.Configuration)
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
            builder.RegisterType<AddressSpaceServices>()
                .AsImplementedInterfaces();
            builder.RegisterType<DataTransferServices>()
                .AsImplementedInterfaces();
            builder.RegisterType<VariantEncoderFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<StackLogger>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .AutoActivate();
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces();

            // Register controllers
            builder.RegisterType<SupervisorMethodsController>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SupervisorSettingsController>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Register supervisor services
            builder.RegisterType<EndpointSupervisorServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<TwinContainerFactory>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            if (_injector != null) {
                // Inject additional services
                builder.RegisterInstance(_injector)
                    .AsImplementedInterfaces()
                    .ExternallyOwned();

                _injector.Inject(builder);
            }

            return builder.Build();
        }

        /// <summary>
        /// Container factory for twins
        /// </summary>
        public class TwinContainerFactory : IContainerFactory {

            /// <summary>
            /// Create twin container factory
            /// </summary>
            /// <param name="client"></param>
            /// <param name="logger"></param>
            /// <param name="injector"></param>
            public TwinContainerFactory(IClientHost client, ILogger logger,
                IInjector injector = null) {
                _client = client ?? throw new ArgumentNullException(nameof(client));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _injector = injector;
            }

            /// <inheritdoc/>
            public IContainer Create(Action<ContainerBuilder> configure) {

                // Create container for all twin level scopes...
                var builder = new ContainerBuilder();

                // Register outer instances
                builder.RegisterInstance(_logger)
                    .ExternallyOwned()
                    .AsImplementedInterfaces();
                builder.RegisterInstance(_client)
                    .ExternallyOwned()
                    .AsImplementedInterfaces();

                // Register other opc ua services
                builder.RegisterType<VariantEncoderFactory>()
                    .AsImplementedInterfaces();
                builder.RegisterType<EndpointTwinServices>()
                    .AsImplementedInterfaces().InstancePerLifetimeScope();
                builder.RegisterType<AddressSpaceServices>()
                    .AsImplementedInterfaces().InstancePerLifetimeScope();
                builder.RegisterType<DataTransferServices>()
                    .AsImplementedInterfaces().InstancePerLifetimeScope();

                // Register module framework
                builder.RegisterModule<ModuleFramework>();
                builder.RegisterModule<IoTEdgeHosting>();
                builder.RegisterModule<LogAnalyticsMetrics>();
                builder.RegisterModule<NewtonSoftJsonModule>();

                // Register twin controllers
                builder.RegisterType<EndpointMethodsController>()
                    .AsImplementedInterfaces().InstancePerLifetimeScope();
                builder.RegisterType<EndpointSettingsController>()
                    .AsImplementedInterfaces().InstancePerLifetimeScope();

                configure?.Invoke(builder);
                _injector?.Inject(builder);

                // Build twin container
                return builder.Build();
            }

            private readonly IClientHost _client;
            private readonly IInjector _injector;
            private readonly ILogger _logger;
        }

        private readonly IConfiguration _config;
        private readonly IInjector _injector;
        private readonly TaskCompletionSource<bool> _exit;
        private TaskCompletionSource<bool> _reset;
        private int _exitCode;
        private static readonly Gauge kTwinModuleStart = Metrics
            .CreateGauge("iiot_edge_twin_module_start", "twin module started",
                new GaugeConfiguration {
                    LabelNames = new[] { "deviceid", "module" }
                });
    }
}
