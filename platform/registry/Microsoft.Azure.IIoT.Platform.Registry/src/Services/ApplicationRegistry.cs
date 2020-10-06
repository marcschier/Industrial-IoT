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
    using Prometheus;
    using Serilog;
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
            IRegistryEventBroker<IApplicationRegistryListener> broker,
            ILogger logger) {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _database = database ?? throw new ArgumentNullException(nameof(database));

            _bulk = bulk ?? throw new ArgumentNullException(nameof(bulk));
            _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationResultModel> RegisterApplicationAsync(
            ApplicationRegistrationRequestModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.ApplicationUri == null) {
                throw new ArgumentException("Missing application uri", nameof(request));
            }

            var context = request.Context.Validate();

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
            RegistryOperationContextModel context, CancellationToken ct) {
            context = context.Validate();

            var application = await _database.DeleteAsync(applicationId, application => {
                if (application.GenerationId != generationId) {
                    return Task.FromException<bool>(
                        new ResourceOutOfDateException("Generation id not matching"));
                }
                return Task.FromResult(false);
            }, ct).ConfigureAwait(false);
            if (application == null) {
                return;
            }
            await _broker.NotifyAllAsync(l => l.OnApplicationDeletedAsync(context,
                applicationId, application)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = request.Context.Validate();

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
            string applicationId, bool filterInactiveTwins, CancellationToken ct) {
            var application = await _database.FindAsync(applicationId, ct).ConfigureAwait(false);
            if (application == null) {
                return null;
            }
            var endpoints = await _endpoints.GetApplicationEndpoints(applicationId, application.NotSeenSince != null, filterInactiveTwins, ct).ConfigureAwait(false);
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
        public async Task PurgeDisabledApplicationsAsync(TimeSpan notSeenSince,
            RegistryOperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            var absolute = DateTime.UtcNow - notSeenSince;
            string continuation = null;
            do {
                var applications = await _database.QueryAsync(null, continuation, null, ct).ConfigureAwait(false);
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
                            context, application.ApplicationId, application)).ConfigureAwait(false);
                    }
                    catch (Exception ex) {
                        _logger.Error(ex, "Exception purging application {id} - continue",
                            found.ApplicationId);
                        continue;
                    }
                }
            }
            while (continuation != null);
        }

        /// <inheritdoc/>
        public Task<ApplicationInfoListModel> QueryApplicationsAsync(
            ApplicationRegistrationQueryModel model, int? pageSize, CancellationToken ct) {
            return _database.QueryAsync(model, null, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task ProcessDiscoveryEventsAsync(string discovererId,
            string supervisorId, DiscoveryResultModel result,
            IEnumerable<DiscoveryEventModel> events) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            if (result == null) {
                throw new ArgumentNullException(nameof(result));
            }
            if (events == null) {
                throw new ArgumentNullException(nameof(events));
            }
            var context = result.Context.Validate();

            //
            // Get all applications for this discoverer or the site the application
            // was found in.  There should only be one site in the found application set
            // or none, otherwise, throw.  The OR covers where site of a discoverer was
            // changed after a discovery run (same discoverer that registered, but now
            // different site reported).
            //
            var existing = await _database.QueryAllAsync(
                new ApplicationRegistrationQueryModel {
                    DiscovererId = discovererId
                }).ConfigureAwait(false);

            //
            // Ensure we set the site id and discoverer id in the found applications
            // have correct values.  Also set application id even though it should
            // automatically happen later
            //
            var eventList = events.ToList();
            eventList.ForEach(ev => {
                ev.Application.DiscovererId = discovererId;
                ev.Application.ApplicationId = ApplicationInfoModelEx.CreateApplicationId(discovererId,
                    ev.Application.ApplicationUri, ev.Application.ApplicationType);
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
                        ev.Endpoint.SupervisorId = supervisorId;
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
                ApplicationInfoModelEx.LogicalEquality);
            var add = new HashSet<ApplicationInfoModel>(found,
                ApplicationInfoModelEx.LogicalEquality);
            var unchange = new HashSet<ApplicationInfoModel>(existing,
                ApplicationInfoModelEx.LogicalEquality);
            var change = new HashSet<ApplicationInfoModel>(found,
                ApplicationInfoModelEx.LogicalEquality);

            unchange.IntersectWith(add);
            change.IntersectWith(remove);
            remove.ExceptWith(found);
            add.ExceptWith(existing);

            var added = 0;
            var updated = 0;
            var unchanged = 0;
            var removed = 0;

            if (!(result.RegisterOnly ?? false)) {
                // Remove applications
                foreach (var removal in remove) {
                    try {
                        // Only touch applications the discoverer owns.
                        if (removal.DiscovererId == discovererId) {
                            var wasUpdated = false;

                            // Disable if not already disabled
                            var application = await _database.UpdateAsync(removal.ApplicationId,
                                application => {
                                    // Disable application
                                    if (application.NotSeenSince == null) {
                                        application.NotSeenSince = DateTime.UtcNow;
                                        application.Updated = context;
                                        removed++;
                                        wasUpdated = true;
                                        return Task.FromResult(true);
                                    }
                                    unchanged++;
                                    return Task.FromResult(false);
                                }).ConfigureAwait(false);

                            if (wasUpdated) {
                                await _broker.NotifyAllAsync(l => l.OnApplicationUpdatedAsync(context, application)).ConfigureAwait(false);
                            }
                        }
                        else {
                            // Skip the ones owned by other discoverers
                            unchanged++;
                        }
                    }
                    catch (Exception ex) {
                        unchanged++;
                        _logger.Error(ex, "Exception during application disabling.");
                    }
                }
            }

            // ... add brand new applications
            foreach (var addition in add) {
                try {
                    var application = addition.Clone();
                    application.Created = context;
                    application.NotSeenSince = null;
                    application = await _database.AddAsync(application).ConfigureAwait(false);

                    // Notify addition!
                    await _broker.NotifyAllAsync(l => l.OnApplicationNewAsync(context, application)).ConfigureAwait(false);

                    // Now - add all new endpoints
                    endpoints.TryGetValue(application.ApplicationId, out var epFound);
                    await _bulk.ProcessDiscoveryEventsAsync(epFound, result, discovererId,
                        supervisorId, null, false).ConfigureAwait(false);
                    added++;
                }
                catch (ResourceConflictException) {
                    unchange.Add(addition); // Update the existing one
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception adding application from discovery.");
                }
            }

            // Update applications and endpoints ...
            foreach (var update in unchange) {
                try {
                    var wasUpdated = false;

                    // Disable if not already disabled
                    var application = await _database.UpdateAsync(update.ApplicationId, application => {
                        //
                        // Check whether another discoverer owns this application (discoverer
                        // id are not the same) and it is not disabled before updating it it.
                        //
                        if (update.DiscovererId != discovererId && application.NotSeenSince == null) {
                            // TODO: Decide whether we merge newly found endpoints...
                            unchanged++;
                            return Task.FromResult(false);
                        }

                        wasUpdated = true;

                        application.Patch(update);
                        application.DiscovererId = discovererId;
                        application.NotSeenSince = null;
                        application.Updated = context;
                        updated++;
                        return Task.FromResult(true);
                    }).ConfigureAwait(false);

                    if (wasUpdated) {
                        // If this is our discoverer's application we update all endpoints also.
                        endpoints.TryGetValue(application.ApplicationId, out var epFound);

                        // TODO: Handle case where we take ownership of all endpoints
                        await _bulk.ProcessDiscoveryEventsAsync(epFound, result, discovererId, supervisorId,
                            update.ApplicationId, false).ConfigureAwait(false);

                        await _broker.NotifyAllAsync(l => l.OnApplicationUpdatedAsync(context, application)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception during update.");
                }
            }
            _logger.Information("... processed discovery results from {discovererId}: " +
                "{added} applications added, {updated} updated, {removed} disabled, and " +
                "{unchanged} unchanged.", discovererId, added, updated, removed, unchanged);
            kAppsAdded.Set(added);
            kAppsUpdated.Set(updated);
            kAppsUnchanged.Set(unchanged);
        }

        private readonly IApplicationRepository _database;
        private readonly ILogger _logger;
        private readonly IEndpointBulkProcessor _bulk;
        private readonly IApplicationEndpointRegistry _endpoints;
        private readonly IRegistryEventBroker<IApplicationRegistryListener> _broker;

        private static readonly Gauge kAppsAdded = Metrics
            .CreateGauge("iiot_registry_applicationAdded", "Number of applications added ");
        private static readonly Gauge kAppsUpdated = Metrics
            .CreateGauge("iiot_registry_applicationsUpdated", "Number of applications updated ");
        private static readonly Gauge kAppsUnchanged = Metrics
            .CreateGauge("iiot_registry_applicationUnchanged", "Number of applications unchanged ");
    }
}
