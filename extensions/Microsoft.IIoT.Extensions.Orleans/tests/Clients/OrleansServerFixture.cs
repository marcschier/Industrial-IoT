// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Clients {
    using Microsoft.IIoT.Extensions.Orleans.Services;
    using Microsoft.IIoT.Utils;
    using System;
    using System.Threading;
    using Autofac;
    using Xunit;

    [CollectionDefinition(Name)]
    public class OrleansServerCollection : ICollectionFixture<OrleansServerFixture> {

        public const string Name = "Server";
    }

    public sealed class OrleansServerFixture : IDisposable {

        public static bool Up => _container != null;

        /// <summary>
        /// Create fixture
        /// </summary>
        public OrleansServerFixture() {
            if (Interlocked.Increment(ref _refcount) == 1) {
                try {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<OrleansSiloHost>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.AddDiagnostics();
                    builder.RegisterType<HostAutoStart>()
                        .AutoActivate()
                        .AsImplementedInterfaces().SingleInstance();

                    _container = builder.Build();
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