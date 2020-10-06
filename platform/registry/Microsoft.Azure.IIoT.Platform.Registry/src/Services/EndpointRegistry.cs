// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Endpoint registry.
    /// </summary>
    public sealed class EndpointRegistry : IEndpointRegistry, IApplicationEndpointRegistry,
        IEndpointBulkProcessor, IApplicationRegistryListener, IDisposable {

        /// <summary>
        /// Create endpoint registry
        /// </summary>
        /// <param name="database"></param>
        /// <param name="broker"></param>
        /// <param name="logger"></param>
        /// <param name="certificates"></param>
        /// <param name="events"></param>
        public EndpointRegistry(IEndpointRepository database,
            IRegistryEventBroker<IEndpointRegistryListener> broker,
            ICertificateServices<EndpointInfoModel> certificates, ILogger logger,
            IRegistryEvents<IApplicationRegistryListener> events = null) {

            _database = database ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Register for application registry events
            _unregister = events?.Register(this);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _unregister?.Invoke();
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> GetEndpointAsync(string endpointId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            return await _database.FindAsync(endpointId, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> ListEndpointsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            return await _database.QueryAsync(null, continuation, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> QueryEndpointsAsync(
            EndpointInfoQueryModel model, int? pageSize,
            CancellationToken ct) {
            return await _database.QueryAsync(model, null, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateEndpointAsync(string endpointId,
            EndpointInfoUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = request.Context.Validate();

            var endpoint = await _database.UpdateAsync(endpointId, existing => {
                if (existing.GenerationId != request.GenerationId) {
                    throw new ResourceOutOfDateException("Generation id no match");
                }
                if (request.ActivationState != null) {
                    existing.ActivationState = request.ActivationState.Value;
                }
                existing.Updated = context;
                return Task.FromResult(true);
            }, ct).ConfigureAwait(false);

            // Send update to through broker
            await _broker.NotifyAllAsync(l => l.OnEndpointUpdatedAsync(context,
                endpoint)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainModel> GetEndpointCertificateAsync(
            string endpointId, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }

            // Get existing endpoint - get should always throw
            var endpoint = await _database.FindAsync(endpointId, ct).ConfigureAwait(false);

            if (endpoint == null) {
                throw new ResourceNotFoundException(
                    $"{endpointId} is not an endpoint registration.");
            }
            if (string.IsNullOrEmpty(endpoint.SupervisorId)) {
                throw new ArgumentException(
                    $"Twin {endpointId} not registered with a supervisor.", nameof(endpointId));
            }

            var rawCertificates = await _certificates.GetEndpointCertificateAsync(
                endpoint, ct).ConfigureAwait(false);
            return rawCertificates.ToCertificateChain();
        }

        /// <inheritdoc/>
        public Task OnApplicationNewAsync(RegistryOperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationUpdatedAsync(RegistryOperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnApplicationDeletedAsync(RegistryOperationContextModel context,
            string applicationId, ApplicationInfoModel application) {
            // Get all endpoint registrations and for each one, call delete, if failure,
            // stop half way and throw and do not complete.
            var endpoints = await GetEndpointsAsync(applicationId, true).ConfigureAwait(false);
            foreach (var endpoint in endpoints) {
                await _database.DeleteAsync(endpoint.Id, ep => Task.FromResult(true)).ConfigureAwait(false);
                await _broker.NotifyAllAsync(l => l.OnEndpointDeletedAsync(context,
                    endpoint.Id, endpoint)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EndpointInfoModel>> GetApplicationEndpoints(
            string applicationId, bool includeDeleted, bool filterInactiveTwins, CancellationToken ct) {
            // Include deleted twins if the application itself is deleted.  Otherwise omit.
            var endpoints = await GetEndpointsAsync(applicationId, includeDeleted,
                ct).ConfigureAwait(false);
            if (!filterInactiveTwins) {
                return endpoints;
            }
            return endpoints.Where(e => e.IsActivated());
        }

        /// <inheritdoc/>
        public async Task ProcessDiscoveryEventsAsync(IEnumerable<EndpointInfoModel> newEndpoints,
            DiscoveryResultModel result, string discovererId, string supervisorId,
            string applicationId, bool hardDelete) {

            if (newEndpoints == null) {
                throw new ArgumentNullException(nameof(newEndpoints));
            }

            var context = result.Context.Validate();
            var found = newEndpoints.ToList();

            var existing = Enumerable.Empty<EndpointInfoModel>();
            if (!string.IsNullOrEmpty(applicationId)) {
                // Merge with existing endpoints of the application
                existing = await GetEndpointsAsync(applicationId, true).ConfigureAwait(false);
            }

            var remove = new HashSet<EndpointInfoModel>(existing,
                EndpointInfoModelEx.Logical);
            var add = new HashSet<EndpointInfoModel>(found,
                EndpointInfoModelEx.Logical);
            var unchange = new HashSet<EndpointInfoModel>(existing,
                EndpointInfoModelEx.Logical);
            var change = new HashSet<EndpointInfoModel>(found,
                EndpointInfoModelEx.Logical);

            unchange.IntersectWith(add);
            change.IntersectWith(remove);
            remove.ExceptWith(found);
            add.ExceptWith(existing);

            var added = 0;
            var updated = 0;
            var unchanged = 0;
            var removed = 0;

            if (!(result.RegisterOnly ?? false)) {
                // Remove or disable an endpoint
                foreach (var item in remove) {
                    try {
                        // Only touch applications the discoverer owns.
                        if (item.DiscovererId == discovererId) {
                            if (hardDelete) {
                                var existingEndpoint = await _database.FindAsync(item.Id).ConfigureAwait(false);
                                await _database.DeleteAsync(item.Id, ep => Task.FromResult(true)).ConfigureAwait(false);
                                await _broker.NotifyAllAsync(l => l.OnEndpointDeletedAsync(context,
                                    item.Id, item)).ConfigureAwait(false);
                            }
                            else if (item.IsDisabled()) {
                                var endpoint = await _database.UpdateAsync(item.Id, existing => {
                                    existing.Patch(item);
                                    return Task.FromResult(true);
                                }).ConfigureAwait(false);
                                await _broker.NotifyAllAsync(
                                    l => l.OnEndpointUpdatedAsync(context, endpoint)).ConfigureAwait(false);
                            }
                            else {
                                unchanged++;
                                continue;
                            }
                            removed++;
                        }
                        else {
                            // Skip the ones owned by other supervisors
                            unchanged++;
                        }
                    }
                    catch (Exception ex) {
                        unchanged++;
                        _logger.Error(ex, "Exception during discovery removal.");
                    }
                }
            }

            // Update endpoints that were disabled
            foreach (var exists in unchange) {
                try {
                    if (exists.DiscovererId == null ||
                        exists.DiscovererId == discovererId) {
                        // Get the new one we will patch over the existing one...
                        var patch = change.First(x =>
                            EndpointInfoModelEx.Logical.Equals(x, exists));
                        if (exists.IsActivated()) {
                            patch.ActivationState = exists.ActivationState;
                        }
                        var endpoint = await _database.UpdateAsync(exists.Id, existing => {
                            existing.Patch(patch);
                            return Task.FromResult(true);
                        }).ConfigureAwait(false);
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointUpdatedAsync(context, endpoint)).ConfigureAwait(false);
                        updated++;
                        continue;
                    }
                    unchanged++;
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception during update.");
                }
            }

            // Add endpoint
            foreach (var item in add) {
                try {
                    var update = false;
                    var endpoint = await _database.AddOrUpdateAsync(item.Id, existing => {
                        update = true;
                        return Task.FromResult(existing.Patch(item));
                    }).ConfigureAwait(false);
                    if (update) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointUpdatedAsync(context, endpoint)).ConfigureAwait(false);
                        updated++;
                        continue;
                    }
                    await _broker.NotifyAllAsync(l => l.OnEndpointNewAsync(context, endpoint)).ConfigureAwait(false);
                    added++;
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception adding endpoint from discovery.");
                }
            }

            if (added != 0 || removed != 0) {
                _logger.Information("processed endpoint results: {added} endpoints added, {updated} " +
                    "updated, {removed} removed or disabled, and {unchanged} unchanged.",
                    added, updated, removed, unchanged);
            }
        }

        /// <summary>
        /// Get all endpoints for application id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EndpointInfoModel>> GetEndpointsAsync(
            string applicationId, bool includeDeleted, CancellationToken ct = default) {
            return await _database.QueryAllAsync(new EndpointInfoQueryModel {
                IncludeNotSeenSince = includeDeleted,
                ApplicationId = applicationId
            }, ct).ConfigureAwait(false);
        }

        private readonly ICertificateServices<EndpointInfoModel> _certificates;
        private readonly IRegistryEventBroker<IEndpointRegistryListener> _broker;
        private readonly IEndpointRepository _database;
        private readonly Action _unregister;
        private readonly ILogger _logger;
    }
}
