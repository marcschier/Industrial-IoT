// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans.Clients {
    using Microsoft.Azure.IIoT.Services.Orleans.Runtime;
    using Microsoft.Azure.IIoT.Services.Orleans.Grains;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using Autofac;
    using Xunit;
    using System.Threading;

    [CollectionDefinition(Name)]
    public class OrleansCollection : ICollectionFixture<OrleansServerFixture> {

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

                    builder.RegisterType<OrleansConfig>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<OrleansSiloHost>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<HostAutoStart>()
                        .AutoActivate()
                        .AsImplementedInterfaces().SingleInstance();

                    builder.AddDiagnostics();
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