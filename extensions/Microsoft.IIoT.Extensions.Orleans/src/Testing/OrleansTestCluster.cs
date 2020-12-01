// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Testing {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System;
    using System.Threading.Tasks;
    using global::Orleans;
    using global::Orleans.TestingHost;

    /// <summary>
    /// Test cluster
    /// </summary>
    public sealed class OrleansTestCluster<TStartup> : IOrleansClientHost, IOrleansGrainClient,
        IOrleansTestCluster, IHostedService, IDisposable where TStartup : OrleansStartup, new() {

        /// <inheritdoc/>
        public IClusterClient Client => Cluster.Client;

        /// <inheritdoc/>
        public IGrainFactory Grains => Cluster.Client;

        /// <inheritdoc/>
        public TestCluster Cluster { get; private set; }

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        public OrleansTestCluster(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken ct) {
            try {
                if (Cluster != null) {
                    throw new ResourceInvalidStateException(
                        "Silo Host already started");
                }

                // define the cluster configuration
                var builder = new TestClusterBuilder()
                    .ConfigureHostConfiguration(ConfigureHostConfiguration)
                    .AddSiloBuilderConfigurator<OrleansTestStartup<TStartup>>()
                    .AddClientBuilderConfigurator<OrleansTestStartup<TStartup>>();
                builder.Options.UseTestClusterMembership = true;
                var cluster = builder.Build();
                if (cluster.Primary == null) {
                    await cluster.DeployAsync().ConfigureAwait(false);
                }
                await cluster.WaitForLivenessToStabilizeAsync().ConfigureAwait(false);
                Cluster = cluster;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start silo host");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken ct) {
            try {
                await Cluster.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop silo host");
                throw;
            }
            finally {
                Cluster = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Cluster.Dispose();
        }

        /// <summary>
        /// Configure test cluster host
        /// </summary>
        /// <param name="builder"></param>
        private static void ConfigureHostConfiguration(IConfigurationBuilder builder) {
           // builder.AddInMemoryCollection(new Dictionary<string, string> {
           //     { "ZooKeeperConnectionString", "127.0.0.1:2181" }
           // });
        }

        private readonly ILogger _logger;
    }
}