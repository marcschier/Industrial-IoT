// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Services {
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Discovery;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Application registry service.
    /// </summary>
    public sealed class ApplicationRegistry : IApplicationRegistry, IApplicationBulkProcessor {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="database"></param>
        /// <param name="endpoints"></param>
        /// <param name="bulk"></param>
        /// <param name="broker"></param>
        /// <param name="logger"></param>
        public ApplicationRegistry(IApplicationRepository database,
            IApplicationEndpointRegistry endpoints, IEndpointBulkProcessor bulk,
            IDiscoveryEventBroker<IApplicationRegistryListener> broker, ILogger logger) {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _database = database ?? throw new ArgumentNullException(nameof(database));

            _bulk = bulk ?? throw new ArgumentNullException(nameof(bulk));
            _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationResultModel> RegisterApplicationAsync(
            ApplicationRegistrationRequestModel request, OperationContextModel context, 
            CancellationToken ct) {
            context = context.Validate();
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.ApplicationUri == null) {
                throw new ArgumentException("Missing application uri", nameof(request));
            }

            var application = await _database.AddAsync(
                request.ToApplicationInfo(context), ct).ConfigureAwait(false);

            await _broker.NotifyAllAsync(
                l => l.OnApplicationNewAsync(context, application)).ConfigureAwait(false);

            return new ApplicationRegistrationResultModel {
                Id = application.ApplicationId
            };
        }

        /// <inheritdoc/>
        public async Task UnregisterApplicationAsync(string applicationId, string generationId,
            OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            var application = await _database.DeleteAsync(applicationId, application => {
                if (application.GenerationId != generationId) {
                    return Task.FromException<bool>(
                        new ResourceOutOfDateException("Generation id not matching"));
                }
                return Task.FromResult(true);
            }, ct).ConfigureAwait(false);
            if (application == null) {
                return;
            }
            if (!application.IsLost()) {
                await _broker.NotifyAllAsync(l => l.OnApplicationLostAsync(context,
                    application)).ConfigureAwait(false);
            }
            await _broker.NotifyAllAsync(l => l.OnApplicationDeletedAsync(context,
                application)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateModel request, OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var application = await _database.UpdateAsync(applicationId, existing => {
                if (existing.GenerationId != request.GenerationId) {
                    throw new ResourceOutOfDateException("Generation id no match");
                }
                // Update from update request
                if (request.ApplicationName != null) {
                    existing.ApplicationName = string.IsNullOrEmpty(request.ApplicationName) ?
                        null : request.ApplicationName;
                }
                if (request.LocalizedNames != null) {
                    existing.LocalizedNames = request.LocalizedNames;
                }
                if (request.ProductUri != null) {
                    existing.ProductUri = string.IsNullOrEmpty(request.ProductUri) ?
                        null : request.ProductUri;
                }
                if (request.GatewayServerUri != null) {
                    existing.GatewayServerUri = string.IsNullOrEmpty(request.GatewayServerUri) ?
                        null : request.GatewayServerUri;
                }
                if (request.Capabilities != null) {
                    existing.Capabilities = request.Capabilities.Count == 0 ?
                        null : request.Capabilities;
                }
                if (request.DiscoveryUrls != null) {
                    existing.DiscoveryUrls = request.DiscoveryUrls.Count == 0 ?
                        null : request.DiscoveryUrls;
                }
                if (request.Locale != null) {
                    existing.Locale = string.IsNullOrEmpty(request.Locale) ?
                        null : request.Locale;
                }
                if (request.DiscoveryProfileUri != null) {
                    existing.DiscoveryProfileUri = string.IsNullOrEmpty(request.DiscoveryProfileUri) ?
                        null : request.DiscoveryProfileUri;
                }
                existing.Updated = context;
                return Task.FromResult(true);
            }, ct).ConfigureAwait(false);

            // Send update to through broker
            await _broker.NotifyAllAsync(l => l.OnApplicationUpdatedAsync(context,
                application)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationModel> GetApplicationAsync(
            string applicationId, CancellationToken ct) {
            var application = await _database.FindAsync(applicationId, ct).ConfigureAwait(false);
            if (application == null) {
                throw new ResourceNotFoundException("Could not find application");
            }
            var endpoints = await _endpoints.GetApplicationEndpointsAsync(applicationId, 
                application.IsLost(), ct).ConfigureAwait(false);
            return new ApplicationRegistrationModel {
                Application = application,
                Endpoints = endpoints.ToList()
            };
        }

        /// <inheritdoc/>
        public Task<ApplicationInfoListModel> ListApplicationsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            return _database.QueryAsync(null, continuation, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task PurgeLostApplicationsAsync(TimeSpan notSeenSince,
            OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            var absolute = DateTime.UtcNow - notSeenSince;
            string continuation = null;
            do {
                var applications = await _database.QueryAsync(null, continuation, null, 
                    ct).ConfigureAwait(false);
                continuation = applications?.ContinuationToken;
                if (applications?.Items == null) {
                    continue;
                }
                foreach (var found in applications.Items) {
                    if (found.NotSeenSince == null ||
                        found.NotSeenSince.Value >= absolute) {
                        // Skip
                        continue;
                    }
                    try {
                        // Delete if disabled state is reflected in the query result
                        var application = await _database.DeleteAsync(found.ApplicationId,
                            a => Task.FromResult(
                                a.NotSeenSince.HasValue &&
                                a.NotSeenSince.Value < absolute), ct).ConfigureAwait(false);
                        if (application == null) {
                            // Skip - already deleted or not satisfying condition
                            continue;
                        }
                        await _broker.NotifyAllAsync(l => l.OnApplicationDeletedAsync(
                            context, application)).ConfigureAwait(false);
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Exception purging application {id} - continue",
                            found.ApplicationId);
                        continue;
                    }
                }
            }
            while (continuation != null);
        }

        /// <inheritdoc/>
        public Task<ApplicationInfoListModel> QueryApplicationsAsync(
            ApplicationInfoQueryModel model, int? pageSize, CancellationToken ct) {
            return _database.QueryAsync(model, null, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task ProcessDiscoveryEventsAsync(string discovererId,
            DiscoveryContextModel result, IEnumerable<DiscoveryResultModel> events) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            if (result == null) {
                throw new ArgumentNullException(nameof(result));
            }
            if (events == null) {
                throw new ArgumentNullException(nameof(events));
            }
            var operationContext = result.Context.Validate();

            // Get all applications.
            var existing = await _database.QueryAllAsync(
                new ApplicationInfoQueryModel {
                    DiscovererId = discovererId
                }).ConfigureAwait(false);

            // Ensure we set set application id even though it should automatically happen later
            var eventList = events.ToList();
            eventList.ForEach(ev => {
                ev.Application.DiscovererId = discovererId;
                ev.Application.SetApplicationId();
            });

            // Create endpoints lookup table per application id
            var endpoints = eventList.GroupBy(k => k.Application.ApplicationId).ToDictionary(
                group => group.Key,
                group => group
                    .Select(ev => {
                        //
                        // Ensure the site id and discoverer id in the found endpoints
                        // have correct values, same as applications earlier.
                        //
                        ev.Endpoint.DiscovererId = discovererId;
                        ev.Endpoint.ApplicationId = group.Key;
                        return ev.Endpoint.Clone();
                    })
                    .ToList());

            //
            // Merge found with existing applications. For disabled applications this will
            // take ownership regardless of discoverer, unfound applications are only disabled
            // and existing ones are patched only if they were previously reported by the same
            // discoverer.  New ones are simply added.
            //
            var found = eventList.Select(ev => ev.Application);
            var remove = new HashSet<ApplicationInfoModel>(existing,
                ApplicationInfoModelEx.Logical);
            var add = new HashSet<ApplicationInfoModel>(found,
                ApplicationInfoModelEx.Logical);
            var unchange = new HashSet<ApplicationInfoModel>(existing,
                ApplicationInfoModelEx.Logical);
            var change = new HashSet<ApplicationInfoModel>(found,
                ApplicationInfoModelEx.Logical);

            unchange.IntersectWith(add);
            change.IntersectWith(remove);
            remove.ExceptWith(found);
            add.ExceptWith(existing);

            var added = 0;
            var updated = 0;
            var unchanged = 0;
            var lost = 0;

            if (!(result.RegisterOnly ?? false)) {
                // Remove applications
                foreach (var item in remove) {
                    try {
                        var wasLost = false;
                        var wasPatched = false;

                        // Marks as not seen
                        var app = await _database.UpdateAsync(item.ApplicationId, existing => {
                            wasLost = existing.IsLost();
                            item.Updated = operationContext;
                            // Disable application
                            item.SetAsLost();
                            wasPatched = existing.Patch(item, out existing);
                            return Task.FromResult(wasPatched);
                        }).ConfigureAwait(false);

                        if (wasPatched) {
                            await _broker.NotifyAllAsync(
                                l => l.OnApplicationUpdatedAsync(operationContext, app)).ConfigureAwait(false);
                            updated++;
                        }
                        else {
                            unchanged++;
                        }
                        if (wasLost) {
                            await _broker.NotifyAllAsync(
                                l => l.OnApplicationLostAsync(operationContext, app)).ConfigureAwait(false);
                            lost++;
                        }
                    }
                    catch (ResourceNotFoundException) {
                        unchanged++; // Can happen if app is already gone
                    }
                    catch (Exception ex) {
                        unchanged++;
                        _logger.LogError(ex, "Exception during application disabling.");
                    }
                }
            }

            // ... add brand new applications
            foreach (var addition in add) {
                try {
                    var app = addition.Clone();
                    app.Created = operationContext;
                    app.Updated = operationContext;
                    app.DiscovererId = discovererId;
                    app.SetApplicationId();
                    app.SetAsFound();
                    var wasAdded = false;
                    var wasPatched = false;
                    var wasFound = false;
                    app = await _database.AddOrUpdateAsync(app.ApplicationId, 
                        existing => {
                            wasAdded = existing == null;
                            wasFound = existing?.IsLost() ?? true;
                            wasPatched = existing.Patch(app, out existing);
                            return Task.FromResult(existing);
                        }).ConfigureAwait(false);

                    if (wasAdded) {
                        // Notify addition!
                        await _broker.NotifyAllAsync(
                            l => l.OnApplicationNewAsync(operationContext, app)).ConfigureAwait(false);
                        added++;
                    }
                    else if (wasPatched) {
                        // Notify update
                        await _broker.NotifyAllAsync(
                            l => l.OnApplicationUpdatedAsync(operationContext, app)).ConfigureAwait(false);
                        updated++;
                    }
                    else {
                        unchanged++;
                    }
                    if (wasFound) {
                        // Notify found
                        await _broker.NotifyAllAsync(
                            l => l.OnApplicationFoundAsync(operationContext, app)).ConfigureAwait(false);
                    }

                    // Now - add all new endpoints
                    endpoints.TryGetValue(app.ApplicationId, out var epFound);
                    await _bulk.ProcessDiscoveryEventsAsync(epFound, result, discovererId,
                        app.ApplicationId).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.LogError(ex, "Exception adding application from discovery.");
                }
            }

            // Update existing applications and endpoints ...
            foreach (var update in unchange) {
                try {
                    var wasFound = false;
                    var wasPatched = false;

                    // Disable if not already disabled
                    var app = await _database.UpdateAsync(update.ApplicationId, existing => {
                        update.SetAsFound();
                        update.Updated = operationContext;

                        wasFound = existing.IsLost();
                        wasPatched = existing.Patch(update, out existing);
                        return Task.FromResult(wasPatched);
                    }).ConfigureAwait(false);

                    endpoints.TryGetValue(app.ApplicationId, out var epFound);
                    await _bulk.ProcessDiscoveryEventsAsync(epFound, result, discovererId,
                        update.ApplicationId).ConfigureAwait(false);

                    if (wasPatched) {
                        updated++;
                        await _broker.NotifyAllAsync(
                            l => l.OnApplicationUpdatedAsync(operationContext, app)).ConfigureAwait(false);
                    }
                    else {
                        unchanged++;
                    }
                    if (wasFound) {
                        await _broker.NotifyAllAsync(
                            l => l.OnApplicationFoundAsync(operationContext, app)).ConfigureAwait(false);
                    }
                }
                catch (ResourceNotFoundException) {
                    unchanged++; // Can happen if app is already gone
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.LogError(ex, "Exception during update.");
                }
            }
            _logger.LogInformation("... processed discovery results from {discovererId}: " +
                "{added} applications added, {updated} updated, {lost} lost, and " +
                "{unchanged} unchanged.", discovererId, added, updated, lost, unchanged);
        }

        private readonly IApplicationRepository _database;
        private readonly ILogger _logger;
        private readonly IEndpointBulkProcessor _bulk;
        private readonly IApplicationEndpointRegistry _endpoints;
        private readonly IDiscoveryEventBroker<IApplicationRegistryListener> _broker;
    }
}
