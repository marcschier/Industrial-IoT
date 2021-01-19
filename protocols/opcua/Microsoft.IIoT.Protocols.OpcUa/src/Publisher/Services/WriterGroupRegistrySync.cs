// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Services {
    using Microsoft.IIoT.Platform.Publisher;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Utils;
    using System.Threading.Tasks;
    using System.Threading;
    using System;

    /// <summary>
    /// Manage writer group and contained writer state
    /// </summary>
    public sealed class WriterGroupRegistrySync : IDataSetWriterStateUpdater,
        IWriterGroupStateUpdater, IEndpointRegistryListener {

        /// <summary>
        /// Create registry management service
        /// </summary>
        /// <param name="dataSets"></param>
        /// <param name="writers"></param>
        /// <param name="groups"></param>
        /// <param name="itemEvents"></param>
        /// <param name="writerEvents"></param>
        /// <param name="groupEvents"></param>
        public WriterGroupRegistrySync(IDataSetEntityRepository dataSets,
            IDataSetWriterRepository writers, IWriterGroupRepository groups,
            IPublisherEventBroker<IPublishedDataSetListener> itemEvents,
            IPublisherEventBroker<IDataSetWriterRegistryListener> writerEvents,
            IPublisherEventBroker<IWriterGroupRegistryListener> groupEvents) {

            _dataSets = dataSets ?? throw new ArgumentNullException(nameof(dataSets));
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));

            _writerEvents = writerEvents ??
                throw new ArgumentNullException(nameof(writerEvents));
            _groupEvents = groupEvents ??
                throw new ArgumentNullException(nameof(groupEvents));
            _itemEvents = itemEvents ??
                throw new ArgumentNullException(nameof(itemEvents));
        }

        /// <inheritdoc/>
        public async Task UpdateDataSetEventStateAsync(string dataSetWriterId,
            PublishedDataSetItemStateModel state, OperationContextModel context,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }
            var updated = false;
            var lastResultChange = state.LastResultChange ?? context?.Time ?? DateTime.UtcNow;
            var result = await _dataSets.UpdateEventDataSetAsync(dataSetWriterId, existing => {
                if (existing?.State != null) {
                    updated = true;
                    existing.State.LastResult = state.LastResult;
                    existing.State.LastResultChange = lastResultChange;
                    existing.State.ServerId = state.ServerId;
                    existing.State.ClientId = state.ClientId;
                }
                else if (state.LastResult != null) {
                    updated = true;
                    existing.State = new PublishedDataSetItemStateModel {
                        LastResult = state.LastResult,
                        LastResultChange = lastResultChange,
                        ClientId = state.ClientId,
                        ServerId = state.ServerId,
                    };
                }
                return Task.FromResult(updated);
            }, ct).ConfigureAwait(false);
            if (updated) {
                // If updated notify about dataset writer change
                await _itemEvents.NotifyAllAsync(
                    l => l.OnPublishedDataSetEventsStateChangeAsync(context,
                        dataSetWriterId, result)).ConfigureAwait(false);
                await _writerEvents.NotifyAllAsync(
                    l => l.OnDataSetWriterStateChangeAsync(context,
                        dataSetWriterId, null)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateDataSetVariableStateAsync(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state,
            OperationContextModel context, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }
            var lastResultChange = state.LastResultChange ?? context?.Time ?? DateTime.UtcNow;
            var updated = false;
            var result = await _dataSets.UpdateDataSetVariableAsync(dataSetWriterId,
                variableId, existing => {
                    if (existing?.State != null) {
                        updated = true;
                        existing.State.LastResult = state.LastResult;
                        existing.State.LastResultChange = lastResultChange;
                        existing.State.ServerId = state.ServerId;
                        existing.State.ClientId = state.ClientId;
                    }
                    else if (state.LastResult != null) {
                        updated = true;
                        existing.State = new PublishedDataSetItemStateModel {
                            LastResult = state.LastResult,
                            LastResultChange = lastResultChange,
                            ClientId = state.ClientId,
                            ServerId = state.ServerId,
                        };
                    }
                    return Task.FromResult(updated);
                }, ct).ConfigureAwait(false);
            if (updated) {
                // If updated notify about dataset writer change
                await _itemEvents.NotifyAllAsync(
                    l => l.OnPublishedDataSetVariableStateChangeAsync(context,
                        dataSetWriterId, result)).ConfigureAwait(false);
                await _writerEvents.NotifyAllAsync(
                    l => l.OnDataSetWriterStateChangeAsync(context,
                        dataSetWriterId, null)).ConfigureAwait(false);
            }
        }


        /// <inheritdoc/>
        public async Task UpdateDataSetWriterStateAsync(string dataSetWriterId,
            PublishedDataSetSourceStateModel state, OperationContextModel context,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }
            var updated = false;
            var lastResultChange = state.LastResultChange ?? context?.Time ?? DateTime.UtcNow;
            var writer = await _writers.UpdateAsync(dataSetWriterId, existing => {
                if (existing?.DataSet?.State != null) {
                    updated = true;
                    if (state.LastResultChange == null && state.ConnectionState != null) {
                        existing.DataSet.State.ConnectionState = state.ConnectionState.Clone();
                    }
                    else {
                        existing.DataSet.State.LastResult = state.LastResult;
                        existing.DataSet.State.LastResultChange = lastResultChange;
                    }
                }
                else if (state.LastResult != null) {
                    updated = true;
                    if (existing.DataSet == null) {
                        existing.DataSet = new PublishedDataSetSourceInfoModel();
                    }
                    existing.DataSet.State = new PublishedDataSetSourceStateModel {
                        LastResult = state.LastResult,
                        LastResultChange = state.ConnectionState == null || state.LastResult != null ?
                            lastResultChange : (DateTime?)null,
                        ConnectionState = state.ConnectionState.Clone()
                    };
                }
                return Task.FromResult(updated);
            }, ct).ConfigureAwait(false);
            if (updated) {
                // If updated notify about dataset writer state change
                await _writerEvents.NotifyAllAsync(
                    l => l.OnDataSetWriterStateChangeAsync(context,
                        dataSetWriterId, writer)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateWriterGroupStateAsync(string writerGroupId,
            WriterGroupStatus? state, OperationContextModel context, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            var updated = false;
            var lastResultChange = context?.Time ?? DateTime.UtcNow;
            var group = await _groups.UpdateAsync(writerGroupId, existing => {
                var existingState = existing.State?.LastState ?? WriterGroupStatus.Disabled;
                var updatedState = state ?? WriterGroupStatus.Disabled;
                if (existingState != WriterGroupStatus.Disabled &&
                    existingState != updatedState) {
                    updated = true;
                    existing.State = new WriterGroupStateModel {
                        LastState = updatedState,
                        LastStateChange = lastResultChange
                    };
                }
                return Task.FromResult(updated);
            }, ct).ConfigureAwait(false);
            if (updated) {
                // If updated notify about group change
                await _groupEvents.NotifyAllAsync(
                    l => l.OnWriterGroupStateChangeAsync(context, group)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public Task OnEndpointNewAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            if (endpoint is null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            return EnableWritersWithEndpointAsync(endpoint.Id, true,
                context == null ? null : new OperationContextModel {
                    AuthorityId = context.AuthorityId,
                    Time = context.Time
                });
        }

        /// <inheritdoc/>
        public Task OnEndpointUpdatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            // No changes required
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointFoundAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            if (endpoint is null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            return EnableWritersWithEndpointAsync(endpoint.Id, true,
                context == null ? null : new OperationContextModel {
                    AuthorityId = context.AuthorityId,
                    Time = context.Time
                });
        }

        /// <inheritdoc/>
        public Task OnEndpointLostAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            if (endpoint is null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            return EnableWritersWithEndpointAsync(endpoint.Id, false,
                context == null ? null : new OperationContextModel {
                    AuthorityId = context.AuthorityId,
                    Time = context.Time
                });
        }

        /// <inheritdoc/>
        public Task OnEndpointDeletedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            if (endpoint is null) {
                throw new ArgumentNullException(nameof(endpoint));
            }

            // TODO: Delete writer 

            return EnableWritersWithEndpointAsync(endpoint.Id, false,
                context == null ? null : new OperationContextModel {
                    AuthorityId = context.AuthorityId,
                    Time = context.Time
                });
        }

        /// <summary>
        /// Enable or disable all writers with endpoint
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="enable"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task EnableWritersWithEndpointAsync(string endpointId, bool enable,
            OperationContextModel context, CancellationToken ct = default) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var results = await _writers.QueryAsync(new DataSetWriterInfoQueryModel {
                EndpointId = endpointId,
                ExcludeDisabled = !enable
            }, null, null, ct).ConfigureAwait(false);
            var continuationToken = results.ContinuationToken;
            do {
                foreach (var writer in results.DataSetWriters) {
                    await Try.Async(() => EnableDataSetWriterAsync(writer.DataSetWriterId, enable,
                        context, ct)).ConfigureAwait(false);
                }
                results = await _writers.QueryAsync(null, continuationToken,
                        null, ct).ConfigureAwait(false);
                continuationToken = results.ContinuationToken;
            }
            while (!string.IsNullOrEmpty(continuationToken));
        }

        /// <summary>
        /// Enable or disable single writer
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="enable"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task EnableDataSetWriterAsync(string dataSetWriterId, bool enable,
            OperationContextModel context, CancellationToken ct = default) {
            var updated = true;
            var writer = await _writers.UpdateAsync(dataSetWriterId, existing => {
                if (existing.IsDisabled == true && enable) {
                    // Remove disable flag and enable
                    existing.IsDisabled = null;
                }
                else if (existing.IsDisabled != true && !enable) {
                    // Now disable enabled item
                    existing.IsDisabled = true;
                }
                else {
                    updated = false;
                }
                return Task.FromResult(updated);
            }, ct).ConfigureAwait(false);
            if (updated) {
                // If updated notify about dataset writer state change
                await _writerEvents.NotifyAllAsync(
                    l => l.OnDataSetWriterStateChangeAsync(context,
                        dataSetWriterId, writer)).ConfigureAwait(false);
            }
        }

        private readonly IDataSetEntityRepository _dataSets;
        private readonly IDataSetWriterRepository _writers;
        private readonly IWriterGroupRepository _groups;
        private readonly IPublisherEventBroker<IPublishedDataSetListener> _itemEvents;
        private readonly IPublisherEventBroker<IDataSetWriterRegistryListener> _writerEvents;
        private readonly IPublisherEventBroker<IWriterGroupRegistryListener> _groupEvents;
    }
}
