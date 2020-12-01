// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Clients {
    using Microsoft.IIoT.Extensions.Orleans.Testing;
    using Microsoft.Extensions.Hosting;
    using System;
    using Autofac;
    using Xunit;
    using System.Threading;

    [CollectionDefinition(Name)]
    public class OrleansClusterCollection : ICollectionFixture<OrleansClusterFixture> {

        public const string Name = "Cluster";
    }

    public sealed class OrleansClusterStartup : OrleansStartup {

        /// <inheritdoc/>
        protected override void Configure(ContainerBuilder builder) {

            // Add silo services to container
            builder.AddDiagnostics();

            base.Configure(builder);
        }
    }

    public sealed class OrleansClusterFixture : IDisposable {

        public static IOrleansGrainClient Client => _container?.Resolve<IOrleansGrainClient>();

        public static IOrleansTestCluster Cluster => _container?.Resolve<IOrleansTestCluster>();

        public static bool Up => _container != null;

        /// <summary>
        /// Create fixture
        /// </summary>
        public OrleansClusterFixture() {
            if (Interlocked.Increment(ref _refcount) == 1) {
                try {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<OrleansTestCluster<OrleansClusterStartup>>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.AddDiagnostics();
                    _container = builder.Build();

                    // Start cluster
                    var cluster = _container.Resolve<IHostedService>();
                    cluster.StartAsync(default).Wait();
                }
                catch {
                    Interlocked.Decrement(ref _refcount);
                    _container = null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (Interlocked.Decrement(ref _refcount) == 0) {
                _container?.Dispose();
                _container = null;
            }
        }

        private static IContainer _container;
        private static int _refcount;
    }
}