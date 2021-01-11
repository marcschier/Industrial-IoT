// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka.Clients {
    using Microsoft.IIoT.Extensions.Kafka;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Linq;
    using Confluent.Kafka;
    using Confluent.Kafka.Admin;

    /// <summary>
    /// Kafka admin
    /// </summary>
    public sealed class KafkaAdminClient : IHealthCheck, IKafkaAdminClient, IDisposable {

        /// <summary>
        /// Create admin client
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="identity"></param>
        public KafkaAdminClient(IOptionsSnapshot<KafkaServerOptions> config, ILogger logger,
            IProcessIdentity identity = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _admin = new AdminClientBuilder(config.Value.ToClientConfig<AdminClientConfig>(
                    identity?.Id ?? Dns.GetHostName()))
                .SetErrorHandler(OnError)
                .SetLogHandler((_, m) => _logger.Log(m))
                .Build();
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken ct) {
            var status = _status ?? HealthCheckResult.Unhealthy();
            if (status.Status != HealthStatus.Healthy) {
                var metaData = await Try.Async(() => Task.Run(
                    () => _admin.GetMetadata(TimeSpan.FromSeconds(3)))).ConfigureAwait(false);
                if (metaData != null) {
                    // Reset to health
                    _status = status = HealthCheckResult.Healthy();
                }
            }
            return status;
        }

        /// <inheritdoc/>
        public async Task EnsureTopicExistsAsync(string topic) {
            try {
                await _admin.CreateTopicsAsync(
                    new TopicSpecification {
                        Name = topic,
                        NumPartitions = _config.Value.Partitions,
                        ReplicationFactor = (short)_config.Value.ReplicaFactor,
                    }.YieldReturn(),
                    new CreateTopicsOptions {
                        OperationTimeout = TimeSpan.FromSeconds(30),
                        RequestTimeout = TimeSpan.FromSeconds(30)
                    }).ConfigureAwait(false);
            }
            catch (CreateTopicsException e) {
                if (e.Results.Count > 0 &&
                    e.Results[0].Error?.Code == ErrorCode.TopicAlreadyExists) {
                    return;
                }
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _admin?.Dispose();
        }

        /// <summary>
        /// Handle error
        /// </summary>
        /// <param name="client"></param>
        /// <param name="error"></param>
        public void OnError(IClient client, Error error) {
            if (error.IsFatal) {
                _status = HealthCheckResult.Unhealthy(error.ToString());
            }
            else if (error.IsError) {
                _status = HealthCheckResult.Degraded(error.ToString());
            }
            else {
                _status = HealthCheckResult.Healthy();
            }
        }

        private HealthCheckResult? _status;
        private readonly ILogger _logger;
        private readonly IAdminClient _admin;
        private readonly IOptionsSnapshot<KafkaServerOptions> _config;
    }
}
