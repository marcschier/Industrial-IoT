// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor.Services {
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.Azure.EventHubs;
    using Microsoft.Azure.EventHubs.Processor;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Implementation of event processor host interface to host event
    /// processors.
    /// </summary>
    public sealed class EventProcessorHost : IDisposable, IEventProcessingHost {

        /// <summary>
        /// Create host wrapper
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="hub"></param>
        /// <param name="config"></param>
        /// <param name="checkpoint"></param>
        /// <param name="lease"></param>
        /// <param name="logger"></param>
        public EventProcessorHost(IEventProcessorFactory factory, ILogger logger,
            IOptions<EventHubConsumerOptions> hub, IOptions<EventProcessorHostOptions> config,
            ICheckpointManager checkpoint = null, ILeaseManager lease = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _lease = lease;
            _checkpoint = checkpoint;
            _lock = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_host != null) {
                    _logger.LogDebug("Event processor host already running.");
                    return;
                }

                _logger.LogDebug("Starting event processor host...");
                var consumerGroup = _hub.Value.ConsumerGroup;
                if (string.IsNullOrEmpty(consumerGroup)) {
                    consumerGroup = "$default";
                }
                _logger.LogInformation("Using Consumer Group: \"{consumerGroup}\"", consumerGroup);
                if (_lease != null && _checkpoint != null) {
                    _host = new EventHubs.Processor.EventProcessorHost(
                        $"host-{Guid.NewGuid()}", _hub.Value.EventHubPath, consumerGroup,
                        GetEventHubConnectionString(out _), _checkpoint, _lease);
                }
                else {
                    var blobConnectionString = _config.Value.GetStorageConnString();
                    if (!string.IsNullOrEmpty(blobConnectionString)) {
                        _host = new EventHubs.Processor.EventProcessorHost(
                            _hub.Value.EventHubPath, consumerGroup,
                            GetEventHubConnectionString(out var eventHub),
                            blobConnectionString,
                            !string.IsNullOrEmpty(_config.Value.LeaseContainerName) ?
                                _config.Value.LeaseContainerName : eventHub.ToSha256Hash());
                    }
                    else {
                        throw new InvalidConfigurationException(
                            "Invalid checkpointing configuration. No storage configured " +
                            "or checkpoint manager/lease manager implementation injected.");
                    }
                }
                await _host.RegisterEventProcessorFactoryAsync(
                    _factory, new EventProcessorOptions {
                        InitialOffsetProvider = s => _config.Value.InitialReadFromEnd ?
                            EventPosition.FromEnqueuedTime(DateTime.UtcNow) :
                            EventPosition.FromStart(),
                        MaxBatchSize = _config.Value.ReceiveBatchSize,
                        ReceiveTimeout = _config.Value.ReceiveTimeout,
                        InvokeProcessorAfterReceiveTimeout = true
                    }).ConfigureAwait(false);
                _logger.LogInformation("Event processor host started.");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error starting event processor host.");
                _host = null;
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_host != null) {
                    _logger.LogDebug("Stopping event processor host...");
                    await _host.UnregisterEventProcessorAsync().ConfigureAwait(false);
                    _host = null;
                    _logger.LogInformation("Event processor host stopped.");
                }
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Error stopping event processor host");
                _host = null;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
            _lock.Dispose();
        }

        /// <summary>
        /// Helper to get connection string and validate configuration
        /// </summary>
        private string GetEventHubConnectionString(out string eventHubPath) {
            if (!string.IsNullOrEmpty(_hub.Value.EventHubConnString)) {
                try {
                    var csb = new EventHubsConnectionStringBuilder(
                        _hub.Value.EventHubConnString);
                    eventHubPath = _hub.Value.EventHubPath;
                    if (string.IsNullOrEmpty(eventHubPath)) {
                        eventHubPath = csb.EntityPath;
                    }
                    if (!string.IsNullOrEmpty(eventHubPath)) {
                        if (_hub.Value.UseWebsockets) {
                            csb.TransportType = TransportType.AmqpWebSockets;
                        }
                        return csb.ToString();
                    }
                }
                catch {
                    throw new InvalidConfigurationException(
                        "Invalid Event hub connection string " +
                        $"{_hub.Value.EventHubConnString} configured.");
                }
            }
            throw new InvalidConfigurationException(
               "No Event hub connection string with entity path configured.");
        }

        private readonly SemaphoreSlim _lock;
        private readonly ILogger _logger;
        private readonly IOptions<EventHubConsumerOptions> _hub;
        private readonly IOptions<EventProcessorHostOptions> _config;
        private readonly IEventProcessorFactory _factory;
        private readonly ILeaseManager _lease;
        private readonly ICheckpointManager _checkpoint;
        private EventHubs.Processor.EventProcessorHost _host;
    }
}
