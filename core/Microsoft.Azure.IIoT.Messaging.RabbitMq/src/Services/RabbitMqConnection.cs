// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq.Services {
    using System;
    using RabbitMQ.Client;
    using System.Collections.Concurrent;

    /// <summary>
    /// Rabbitmq connection
    /// </summary>
    public sealed class RabbitMqConnection : IRabbitMqConnection, IDisposable {

        /// <summary>
        /// Create connection
        /// </summary>
        /// <param name="config"></param>
        public RabbitMqConnection(IRabbitMqConfig config) {

            var factory = new ConnectionFactory {
                HostName = config.HostName,
                Password = config.Key,
                UserName = config.UserName,

                AutomaticRecoveryEnabled = true,
            };

            _connection = factory.CreateConnection();
        }

        /// <inheritdoc/>
        public IModel GetChannel(string target) {
            return _channels.GetOrAdd(target, t => {
                var channel = _connection.CreateModel();
                channel.QueueDeclare(t);
                return channel;
            });
        }

        /// <inheritdoc/>
        public void Dispose() {
            _connection?.Dispose();
        }

        private readonly IConnection _connection;
        private readonly ConcurrentDictionary<string, IModel> _channels =
            new ConcurrentDictionary<string, IModel>();
    }
}
