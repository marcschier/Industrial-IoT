// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Service {
    using Microsoft.IIoT.Azure.ActiveDirectory;
    using Microsoft.IIoT.Azure.AppInsights;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Autofac.Extensions.DependencyInjection;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Mono app startup
    /// </summary>
    public class Startup {

        /// <summary>
        /// Configuration - Initialized in constructor
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Hosting environment
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Create startup
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration) {
            Environment = env;
            Configuration = configuration;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public void ConfigureServices(IServiceCollection services) {
            if (services is null) {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHeaderForwarding();
            services.AddHttpContextAccessor();
            services.AddHealthChecks();
            services.AddDistributedMemoryCache();
            services.AddApiVersioning();
            services.AddAppInsightsTelemetry();
        }

        /// <summary>
        /// Configure the application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime) {
            if (app is null) {
                throw new ArgumentNullException(nameof(app));
            }
            if (appLifetime is null) {
                throw new ArgumentNullException(nameof(appLifetime));
            }

            var applicationContainer = app.ApplicationServices.GetAutofacRoot();

            app.UsePathBase();
            app.UseHeaderForwarding();
            app.UseHttpsRedirect();

            // Configure branches for business
            app.UseWelcomePage("/");

            // Minimal API surface
            app.AddStartupBranch<Twin.Service.Startup>("/twin");
            app.AddStartupBranch<Discovery.Service.Startup>("/registry");
            app.AddStartupBranch<Publisher.Service.Startup>("/publisher");
            app.AddStartupBranch<Api.Events.Service.Startup>("/events");

            if (!Configuration.GetValue<string>(PcsVariable.PCS_DEPLOYMENT_LEVEL)
                .EqualsIgnoreCase("minimum")) {
                app.AddStartupBranch<Vault.Service.Startup>("/vault");
                app.AddStartupBranch<Twin.Ua.Service.Startup>("/ua");
            }

            app.UseHealthChecks("/healthz");

            // Start processors
            applicationContainer.Resolve<IHostProcess>().StartAsync().Wait();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(applicationContainer.Dispose);
        }

        /// <summary>
        /// Configure Autofac container
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder) {

            // Register service info and configuration interfaces
            builder.AddConfiguration(Configuration);

            // Add diagnostics and auth providers
            builder.AddAppInsightsLogging();
            builder.RegisterModule<ActiveDirectorySupport>();

            builder.RegisterType<ProcessorHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Injected processor host
        /// </summary>
        private sealed class ProcessorHost : IHostProcess, IDisposable, IHealthCheck {

            /// <inheritdoc/>
            public void Start() {
                _cts = new CancellationTokenSource();

                var args = Array.Empty<string>();

                // Minimal processes
                var processes = new List<Task> {
                    Task.Run(() => Publisher.Processor.Program.Main(args), _cts.Token),
                    Task.Run(() => Edge.Events.Service.Program.Main(args), _cts.Token),
                    Task.Run(() => Edge.Tunnel.Service.Program.Main(args), _cts.Token),
                };
                _runner = Task.WhenAll(processes.ToArray());
            }

            /// <inheritdoc/>
            public async Task StartAsync() {
                // Delay start by 10 seconds to let api boot up first
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                Start();
            }

            /// <inheritdoc/>
            public async Task StopAsync() {
                _cts.Cancel();
                try {
                    await _runner.ConfigureAwait(false);
                }
                catch (AggregateException aex) {
                    if (aex.InnerExceptions.All(e => e is OperationCanceledException)) {
                        return;
                    }
                    throw;
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                Try.Async(StopAsync).Wait();
                _cts?.Dispose();
            }

            /// <inheritdoc/>
            public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
                CancellationToken cancellationToken) {
                return Task.FromResult(_runner == null || !_runner.IsFaulted ?
                    HealthCheckResult.Healthy() :
                    new HealthCheckResult(HealthStatus.Unhealthy, null, _runner.Exception));
            }

            private Task _runner;
            private CancellationTokenSource _cts;
        }
    }
}