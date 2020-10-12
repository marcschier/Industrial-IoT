﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO;
    using RabbitMQ.Client;

    /// <summary>
    /// Rabbitmq health checks
    /// </summary>
    public sealed class RabbitMqHealthCheck : IHealthCheck {

        /// <summary>
        /// Create health check
        /// </summary>
        /// <param name="config"></param>
        public RabbitMqHealthCheck(IRabbitMqConfig config) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken) {
            try {
                var factory = new ConnectionFactory {
                    HostName = _config.HostName,
                    Password = _config.Key,
                    UserName = _config.UserName
                };
                using var connection = factory.CreateConnection();
                if (connection == null) {
                    throw new IOException("Couldnt get connection");
                }
                using var model = connection.CreateModel();
                if (model == null || model.IsClosed) {
                    throw new IOException("Couldnt get channel");
                }
                model.ExchangeDelete("test", true);
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex) {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Cannot connect", ex));
            }
        }

        private readonly IRabbitMqConfig _config;
    }
}
