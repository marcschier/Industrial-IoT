// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Clients {
    using Microsoft.Azure.IIoT.Services.Kafka.Runtime;
    using Microsoft.Azure.IIoT.Services.Kafka.Server;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using Autofac;
    using Xunit;
    using System.Threading;

    [CollectionDefinition(Name)]
    public class KafkaCollection : ICollectionFixture<KafkaServerFixture> {

        public const string Name = "Server";
    }

    public sealed class KafkaServerFixture : IDisposable {

        public static bool Up => _container != null;

        /// <summary>
        /// Create fixture
        /// </summary>
        public KafkaServerFixture() {
            if (Interlocked.Increment(ref _refcount) == 1) {
                try {
                    var builder = new ContainerBuilder();

                    builder.RegisterModule<KafkaProducerModule>();
                    builder.RegisterType<KafkaServerConfig>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<KafkaCluster>()
                        .AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<HostAutoStart>()
                        .AutoActivate()
                        .AsImplementedInterfaces().SingleInstance();

                    builder.AddDebugDiagnostics();
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