// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Persistent twin twins registry.
    /// </summary>
    public sealed class TwinRegistry : ITwinRegistry, IEndpointRegistryListener,
        IDisposable {

        /// <summary>
        /// Create twin registry
        /// </summary>
        /// <param name="database"></param>
        /// <param name="endpoints"></param>
        /// <param name="logger"></param>
        /// <param name="broker"></param>
        /// <param name="events"></param>
        public TwinRegistry(ITwinRepository database, IEndpointRegistry endpoints,
            ILogger logger, ITwinEventBroker<ITwinRegistryListener> broker,
            IRegistryEvents<IEndpointRegistryListener> events = null) {

            _database = database ?? throw new ArgumentNullException(nameof(database));
            _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Register for application registry events
            _unregister = events?.Register(this);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _unregister?.Invoke();
        }

        /// <inheritdoc/>
        public Task<TwinActivationResultModel> ActivateTwinAsync(
            TwinActivationRequestModel request, CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<TwinModel> GetTwinAsync(string twinId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var twin = await _database.FindAsync(twinId, ct).ConfigureAwait(false);
            if (twin == null) {
                throw new ResourceNotFoundException("Twin not found");
            }
            var endpoint = await _endpoints.FindEndpointAsync(twin.EndpointId, 
                ct).ConfigureAwait(false);
            if (endpoint == null) {
                // TODO: Consider deleting the twin
                throw new ResourceNotFoundException("Twin endpoint was not found");
            }
            return new TwinModel {
                Created = twin.Created,
                Updated = twin.Updated,
                GenerationId = twin.GenerationId,
                Id = twin.Id,
                ConnectionState = twin.ConnectionState,
                Connection = new ConnectionModel {
                    Endpoint = endpoint.Endpoint,
                    Diagnostics = twin.Diagnostics,
                    OperationTimeout = twin.OperationTimeout,
                    User = twin.User
                }
            };
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListModel> ListTwinsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            return await _database.QueryAsync(null, 
                continuation, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListModel> QueryTwinsAsync(
            TwinInfoQueryModel model, int? pageSize,
            CancellationToken ct) {
            return await _database.QueryAsync(model, null, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateTwinAsync(string twinId,
            TwinInfoUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = request.Context.Validate();

            var twin = await _database.UpdateAsync(twinId, existing => {
                if (existing.GenerationId != request.GenerationId) {
                    throw new ResourceOutOfDateException("Generation id no match");
                }
                if (request.OperationTimeout != null) {
                    existing.OperationTimeout = request.OperationTimeout.Value == TimeSpan.Zero ?
                        null : request.OperationTimeout;
                }
                if (request.User != null) {
                    existing.User = request.User.Type == CredentialType.None ?
                        null : request.User;
                }
                if (request.Diagnostics != null) {
                    existing.Diagnostics = request.Diagnostics;
                }
                existing.Updated = context;
                return Task.FromResult(true);
            }, ct).ConfigureAwait(false);

            // Send update to through broker
            await _broker.NotifyAllAsync(l => l.OnTwinUpdatedAsync(context,
                twin)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task DectivateTwinAsync(string twinId, string generationId, CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task OnEndpointNewAsync(OperationContextModel context, 
            EndpointInfoModel endpoint) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointUpdatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {

            // TODO: Certificate updated - should update twin instance

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnEndpointDeletedAsync(OperationContextModel context,
            string endpointId, EndpointInfoModel endpoint) {
            // Get all twin registrations and for each one, call delete, if failure,
            // stop half way and throw and do not complete.
            var twins = await _database.QueryAllAsync(new TwinInfoQueryModel {
                EndpointId = endpointId
            }).ConfigureAwait(false);
            foreach (var twin in twins) {
                await DeactivateAsync(context, twin).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deactivate twin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        private async Task DeactivateAsync(OperationContextModel context, TwinInfoModel twin) {
            await _database.DeleteAsync(twin.Id,
                ep => Task.FromResult(true)).ConfigureAwait(false);
            await _broker.NotifyAllAsync(l => l.OnTwinDeactivatedAsync(context,
                twin.Id, twin)).ConfigureAwait(false);
        }

        private readonly ITwinEventBroker<ITwinRegistryListener> _broker;
        private readonly ITwinRepository _database;
        private readonly IEndpointRegistry _endpoints;
        private readonly Action _unregister;
        private readonly ILogger _logger;
    }
}
