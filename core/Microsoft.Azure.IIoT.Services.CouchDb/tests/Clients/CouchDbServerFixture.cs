// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.CouchDb.Clients {
    using Microsoft.Azure.IIoT.Services.CouchDb.Server;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Azure.IIoT.Diagnostics;
    using System;
    using Xunit;
    using Autofac;
    using Microsoft.Azure.IIoT.Services.CouchDb.Runtime;

    [CollectionDefinition(Name)]
    public class CouchDbServerCollection : ICollectionFixture<CouchDbServerFixture> {

        public const string Name = "Server";
    }

    public sealed class CouchDbServerFixture : IDisposable {

        public static bool Up { get; private set; }

        /// <summary>
        /// Create fixture
        /// </summary>
        public CouchDbServerFixture() {
            try {
                var builder = new ContainerBuilder();
                builder.RegisterModule<CouchDbModule>();
                builder.RegisterType<CouchDbConfig>()
                    .AsImplementedInterfaces();
                builder.AddDebugDiagnostics();
                _container = builder.Build();
                var healthCheck = _container.Resolve<IHealthCheck>();
                _server = new CouchDbServer(ConsoleLogger.CreateLogger(),
                    check: healthCheck);
                _server.StartAsync().GetAwaiter().GetResult();
                Up = true;
            }
            catch (Exception) {
                _server = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _server?.Dispose();
            _container?.Dispose();
            Up = false;
        }

        private readonly CouchDbServer _server;
        private readonly IContainer _container;
    }
}