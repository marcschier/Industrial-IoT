// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Confluent.Kafka {
    using Microsoft.Azure.IIoT.Services.Kafka;
    using System;

    /// <summary>
    /// Client configuration extensions
    /// </summary>
    internal static class ClientConfigEx {

        /// <summary>
        /// Create configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T ToClientConfig<T>(this IKafkaServerConfig config,
            string clientId = null)
            where T : ClientConfig, new() {
            if (string.IsNullOrEmpty(config?.BootstrapServers)) {
                throw new ArgumentException(nameof(config));
            }
            return new T {
                BootstrapServers = config.BootstrapServers,
                ClientId = clientId,
                // ...
            };
        }
    }
}
