// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Clients {
    using Microsoft.IIoT.Extensions.CouchDb.Server;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using System;
    using Xunit;
    using Autofac;
    using Microsoft.IIoT.Extensions.CouchDb.Runtime;

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
                builder.AddDiagnostics();
                _container = builder.Build();
                var healthCheck = _container.Resolve<IHealthCheck>();
                _server = new CouchDbServer(Log.Console(),
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