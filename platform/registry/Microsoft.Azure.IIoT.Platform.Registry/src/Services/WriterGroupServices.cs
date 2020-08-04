// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// Publisher registry which uses the IoT Hub twin services for publisher
    /// and writer group identity management.
    /// </summary>
    public sealed class WriterGroupServices : IWriterGroupSync, IWriterGroupOrchestration,
        IWriterGroupRegistryListener, IDataSetWriterRegistryListener {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="serializer"></param>
        /// <param name="gateways"></param>
        /// <param name="activation"></param>
        /// <param name="logger"></param>
        public WriterGroupServices(IDeviceTwinServices iothub, IJsonSerializer serializer,
            IGatewayRegistry gateways, IActivationServices<WriterGroupPlacementModel> activation,
            ILogger logger) {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activation = activation ?? throw new ArgumentNullException(nameof(activation));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
        }

        /// <inheritdoc/>
        public async Task SynchronizeWriterGroupPlacementsAsync(CancellationToken ct) {
            // Find all writer groups currently not connected
            var query = $"SELECT * FROM devices WHERE status = 'enabled' AND " +
                $"connectionState != 'Connected' AND " +
                $"tags.{nameof(EntityRegistration.DeviceType)} = '{IdentityType.WriterGroup}' ";

            var result = new List<DeviceTwinModel>();
            string continuation = null;
            do {
                var devices = await _iothub.QueryDeviceTwinsAsync(query, null, null, ct);
                foreach (var writerGroup in devices.Items.Select(d => d.ToWriterGroupRegistration(false))) {
                    await PlaceWriterGroupAsync(writerGroup, ct);
                }
                continuation = devices.ContinuationToken;
            }
            while (continuation != null);
        }

        /// <inheritdoc/>
        public async Task SynchronizeWriterGroupAsync(WriterGroupInfoModel writerGroup,
            IEnumerable<DataSetWriterInfoModel> writers, CancellationToken ct) {
            if (writerGroup == null) {
                throw new ArgumentNullException(nameof(writerGroup));
            }
            if (writers == null) {
                throw new ArgumentNullException(nameof(writers));
            }

            // TODO: Lock synchronize this with the listener callbacks

            var disabled = writerGroup.State == null ||
                writerGroup.State.State == WriterGroupState.Disabled;

            var twin = writerGroup.ToWriterGroupRegistration(disabled).ToDeviceTwin(_serializer);
            twin = await _iothub.CreateOrUpdateAsync(twin, true, ct);
            var configuredWriters = new HashSet<string>();
            foreach (var prop in twin.Properties.Desired.Keys) {
                if (PublisherRegistryEx.IsDataSetWriterProperty(prop)) {
                    configuredWriters.Add(PublisherRegistryEx.ToDataSetWriterId(prop));
                }
            }
            foreach (var dataSetWriterId in writers.Select(w => w.DataSetWriterId)) {
                if (!configuredWriters.Remove(dataSetWriterId)) {
                    // Should be added because it is not yet in the list
                    await AddRemoveWriterFromWriterGroupTwinAsync(
                        PublisherRegistryEx.ToDeviceId(writerGroup.WriterGroupId),
                        dataSetWriterId, false);
                }
            }
            foreach (var dataSetWriterId in configuredWriters) {
                // The rest should be removed
                await AddRemoveWriterFromWriterGroupTwinAsync(
                    PublisherRegistryEx.ToDeviceId(writerGroup.WriterGroupId),
                    dataSetWriterId, true);
            }
        }

        /// <inheritdoc/>
        public async Task OnDataSetWriterAddedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            var writerGroupId = dataSetWriter?.WriterGroupId;
            if (string.IsNullOrEmpty(writerGroupId)) {
                // Should not happen
                throw new ArgumentNullException(nameof(dataSetWriter.WriterGroupId));
            }
            await AddRemoveWriterFromWriterGroupTwinAsync(
                PublisherRegistryEx.ToDeviceId(writerGroupId), dataSetWriter.DataSetWriterId,
                dataSetWriter.IsDisabled ?? false);
        }

        /// <inheritdoc/>
        public async Task OnDataSetWriterUpdatedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter) {
            var writerGroupId = dataSetWriter?.WriterGroupId;
            if (string.IsNullOrEmpty(writerGroupId)) {
                //
                // The variable and event updates do not carry a full model but we can quickly
                // get the missing information by finding twins where this dataset is defined
                //      - should only be one -
                // and patch them
                //
                var twins = await _iothub.QueryAllDeviceTwinsAsync(
                    $"SELECT * FROM devices WHERE " +
                    $"IS_DEFINED(properties.desired.{IdentityType.DataSet}_{dataSetWriterId}) AND " +
                    $"tags.{nameof(EntityRegistration.DeviceType)} = '{IdentityType.WriterGroup}' ");
                foreach (var twin in twins) {
                    await AddRemoveWriterFromWriterGroupTwinAsync(twin.Id, dataSetWriterId,
                        dataSetWriter.IsDisabled ?? false);
                }
            }
            else {
                await AddRemoveWriterFromWriterGroupTwinAsync(
                    PublisherRegistryEx.ToDeviceId(writerGroupId), dataSetWriterId,
                    dataSetWriter.IsDisabled != false);
            }
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterStateChangeAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel writer) {
            // Not interested
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnDataSetWriterRemovedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            if (string.IsNullOrEmpty(dataSetWriter?.WriterGroupId)) {
                // Should not happen
                throw new ArgumentNullException(nameof(dataSetWriter.WriterGroupId));
            }
            await AddRemoveWriterFromWriterGroupTwinAsync(
                PublisherRegistryEx.ToDeviceId(dataSetWriter.WriterGroupId),
                dataSetWriter.DataSetWriterId, true);
        }

        /// <inheritdoc/>
        public Task OnWriterGroupStateChangeAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            // Dont care - registry is the source of state changes
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnWriterGroupAddedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (string.IsNullOrEmpty(writerGroup?.WriterGroupId)) {
                // Should not happen
                throw new ArgumentNullException(nameof(writerGroup.WriterGroupId));
            }

            // Add new but disabled group (or update)
            _logger.Debug("Add group {id} event - Create disabled twin...",
                writerGroup.WriterGroupId);
            var group = writerGroup.ToWriterGroupRegistration(true);
            await _iothub.CreateOrUpdateAsync(group.ToDeviceTwin(_serializer), false);
            _logger.Debug("Add group {id} event - Disabled twin added...",
                writerGroup.WriterGroupId);
        }

        /// <inheritdoc/>
        public async Task OnWriterGroupUpdatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (writerGroup?.WriterGroupId == null) {
                // Should not happen
                throw new ArgumentNullException(nameof(writerGroup.WriterGroupId));
            }
            _logger.Debug("Update group {id} event - Update twin...",
                writerGroup.WriterGroupId);
            while (true) {
                try {
                    var twin = await _iothub.FindAsync(
                        PublisherRegistryEx.ToDeviceId(writerGroup.WriterGroupId));
                    if (twin == null) {
                        _logger.Warning("Missed add group event - try recreating disabled twin...");
                        twin = await _iothub.CreateOrUpdateAsync(
                            writerGroup.ToWriterGroupRegistration(true).ToDeviceTwin(_serializer),
                            false, CancellationToken.None);
                        return; // done
                    }
                    // Convert to writerGroup registration
                    var registration = twin.ToEntityRegistration() as WriterGroupRegistration;
                    if (registration == null) {
                        _logger.Fatal("Unexpected - twin is not a writerGroup registration.");
                        return; // nothing else to do other than delete and recreate.
                    }
                    twin = await _iothub.PatchAsync(registration.Patch(
                        writerGroup.ToWriterGroupRegistration(), _serializer));
                    break;
                }
                catch (ResourceOutOfDateException ex) {
                    // Retry create/update
                    _logger.Debug(ex, "Retry updating writerGroup...");
                }
            }
            _logger.Debug("Update group {id} event - Twin updated.",
                writerGroup.WriterGroupId);
        }

        /// <inheritdoc/>
        public async Task OnWriterGroupActivatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (string.IsNullOrEmpty(writerGroup?.WriterGroupId)) {
                // Should not happen
                throw new ArgumentNullException(nameof(writerGroup.WriterGroupId));
            }

            // Enable group
            _logger.Debug("Activate group {id} event - Enable and place twin...",
                writerGroup.WriterGroupId);
            var group = writerGroup.ToWriterGroupRegistration(false);
            await _iothub.CreateOrUpdateAsync(group.ToDeviceTwin(_serializer), true);

            // Immediately try assign writer group to a publisher
            await PlaceWriterGroupAsync(group, CancellationToken.None);
            _logger.Debug("Activate group {id} event - Twin placed.",
                writerGroup.WriterGroupId);
        }

        /// <inheritdoc/>
        public async Task OnWriterGroupDeactivatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (writerGroup == null) {
                // Should not happen
                throw new ArgumentNullException(nameof(writerGroup));
            }

            // Disable group
            _logger.Debug("Deactivate group {id} event - Disable twin...",
                writerGroup.WriterGroupId);
            var group = writerGroup.ToWriterGroupRegistration(true);
            await _iothub.CreateOrUpdateAsync(group.ToDeviceTwin(_serializer), true);
            _logger.Debug("Deactivate group {id} event - Twin disabled.",
                writerGroup.WriterGroupId);
        }

        /// <inheritdoc/>
        public async Task OnWriterGroupRemovedAsync(PublisherOperationContextModel context,
            string writerGroupId) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                // Should not happen
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            try {
                // Force delete the group
                _logger.Debug("Remove group {id} event - Delete twin...",
                    writerGroupId);
                await _iothub.DeleteAsync(
                    PublisherRegistryEx.ToDeviceId(writerGroupId));
                _logger.Debug("Remove group {id} event - Twin deleted.",
                    writerGroupId);
            }
            catch (Exception ex) {
                // Retry create/update
                _logger.Error(ex, "Remove group {id} event - Deleting twin failed...",
                    writerGroupId);
            }
        }

        /// <summary>
        /// Add or remove the writer from the group
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        private async Task AddRemoveWriterFromWriterGroupTwinAsync(string deviceId,
            string dataSetWriterId, bool remove = false) {
            try {
                _logger.Debug("Adding writer {writer} to writer table in writerGroup {id}.",
                    dataSetWriterId, deviceId);
                await _iothub.PatchAsync(new DeviceTwinModel {
                    Id = deviceId,
                    Properties = new TwinPropertiesModel {
                        Desired = new Dictionary<string, VariantValue> {
                            [PublisherRegistryEx.ToPropertyName(dataSetWriterId)] =
                                remove ? null : DateTime.UtcNow.ToString()
                        }
                    }
                });
                _logger.Debug("Added writer {writer} to writer table in writerGroup {id}.",
                    dataSetWriterId, deviceId);
            }
            catch (Exception ex) {
                // Retry create/update
                _logger.Error(ex,
                    "Adding writer {writer} to writer table in writerGroup {id} failed.",
                    dataSetWriterId, deviceId);
            }
        }

        /// <summary>
        /// Try to activate endpoint on any supervisor in site
        /// </summary>
        /// <param name="writerGroup"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<bool> PlaceWriterGroupAsync(WriterGroupRegistration writerGroup,
            CancellationToken ct) {
            try {
                if (string.IsNullOrEmpty(writerGroup?.SiteId)) {
                    _logger.Error(
                        "Writer group {writerGroupId} is null or has no site assigned!",
                        writerGroup?.WriterGroupId);
                    return false;
                }

                // Get registration
                var writerGroupDevice = await _iothub.GetRegistrationAsync(
                    PublisherRegistryEx.ToDeviceId(writerGroup.WriterGroupId), null, ct);
                if (string.IsNullOrEmpty(writerGroupDevice?.Authentication?.PrimaryKey)) {
                    // No writer group registration
                    return false;
                }

                if (writerGroupDevice.IsDisabled() ?? false) {
                    // Writer group is disabled
                    return true;
                }

                if (writerGroupDevice.IsConnected() ?? false) {
                    // Query state is delayed and writer group is already connected
                    return true;
                }

                // Get all gateways in site
                var gatewaysInSite = await _gateways.QueryAllGatewaysAsync(
                    new GatewayQueryModel { SiteId = writerGroup.SiteId, Connected = true });
                var candidateGateways = gatewaysInSite.Select(s => s.Id).ToList();
                if (candidateGateways.Count == 0) {
                    // No candidates found to assign to
                    _logger.Warning(
                        "Found no gateways in {SiteId} to assign writer group {writerGroupId}!",
                        writerGroup.SiteId, writerGroup.WriterGroupId);

                    // TODO: Consider Update writer group state to flag site has no gateways
                    return false;
                }

                // Loop through all randomly and try to take one that works.
                foreach (var gatewayId in candidateGateways.Shuffle()) {
                    var gateway = await _gateways.FindGatewayAsync(gatewayId, false, ct);
                    var publisherId = gateway?.Modules?.Publisher?.Id;
                    if (string.IsNullOrEmpty(publisherId)) {
                        // No publisher in gateway
                        continue;
                    }
                    try {
                        await _activation.ActivateAsync(new WriterGroupPlacementModel {
                            // Writer group device id
                            WriterGroupId = writerGroup.WriterGroupId,
                            PublisherId = publisherId,
                        }, writerGroupDevice.Authentication.PrimaryKey, ct);
                        _logger.Information(
                            "Activated writer group {writerGroupId} on publisher {publisherId}!",
                             writerGroup.WriterGroupId, publisherId);

                        // Done - writer group was assigned
                        return true;
                    }
                    catch (Exception ex) {
                        _logger.Debug(ex, "Failed to activate writer group" +
                            " {writerGroupId} on Publisher {publisherId} - trying next...",
                             writerGroup.WriterGroupId, publisherId);
                    }
                }
                // Failed
                return false;
            }
            catch (Exception ex) {
                _logger.Debug(ex, "Failed to activate writer group {writerGroupId}. ",
                    writerGroup.WriterGroupId);
                return false;
            }
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IJsonSerializer _serializer;
        private readonly IGatewayRegistry _gateways;
        private readonly ILogger _logger;
        private readonly IActivationServices<WriterGroupPlacementModel> _activation;
    }
}
