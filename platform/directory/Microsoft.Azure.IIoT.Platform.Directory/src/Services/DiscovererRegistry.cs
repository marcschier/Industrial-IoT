// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Services {
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
    using Microsoft.Azure.IIoT.Platform.Directory;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Discoverer registry which uses the IoT Hub twin services for discoverer
    /// identity management.
    /// </summary>
    public sealed class DiscovererRegistry : IDiscovererRegistry {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="broker"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public DiscovererRegistry(IDeviceTwinServices iothub,
            IDirectoryEventBroker<IDiscovererRegistryListener> broker,
            IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<DiscovererModel> GetDiscovererAsync(string id,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            var deviceId = HubResource.Parse(id, out var hub, out var moduleId);
            var device = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            var registration = device.ToEntityRegistration()
                as DiscovererRegistration;
            if (registration == null) {
                throw new ResourceNotFoundException(
                    $"{id} is not a discoverer registration.");
            }
            return registration.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdateDiscovererAsync(string discovererId,
            DiscovererUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }

            // Get existing endpoint and compare to see if we need to patch.
            var deviceId = HubResource.Parse(discovererId, out var hub, out var moduleId);

            while (true) {
                try {
                    var twin = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
                    if (twin.Id != deviceId && twin.ModuleId != moduleId) {
                        throw new ArgumentException("Id must be same as twin to patch",
                            nameof(discovererId));
                    }

                    var registration = twin.ToEntityRegistration(true) as DiscovererRegistration;
                    if (registration == null) {
                        throw new ResourceNotFoundException(
                            $"{discovererId} is not a discoverer registration.");
                    }

                    // Update registration from update request
                    var patched = registration.ToServiceModel();

                    if (request.LogLevel != null) {
                        patched.LogLevel = request.LogLevel == TraceLogLevel.Information ?
                            null : request.LogLevel;
                    }

                    // Patch
                    twin = await _iothub.PatchAsync(registration.Patch(
                        patched.ToDiscovererRegistration(), _serializer), 
                        false, ct).ConfigureAwait(false);

                    // Send update to through broker
                    registration = twin.ToEntityRegistration(true) as DiscovererRegistration;
                    await _broker.NotifyAllAsync(l => l.OnDiscovererUpdatedAsync(null,
                        registration.ToServiceModel())).ConfigureAwait(false);
                    return;
                }
                catch (ResourceOutOfDateException ex) {
                    _logger.Debug(ex, "Retrying updating discoverer...");
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DiscovererListModel> ListDiscoverersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Discoverer}' " +
                $"AND NOT IS_DEFINED(tags.{nameof(EntityRegistration.NotSeenSince)})";
            var devices = await _iothub.QueryDeviceTwinsAsync(query, 
                continuation, pageSize, ct).ConfigureAwait(false);
            return new DiscovererListModel {
                ContinuationToken = devices.ContinuationToken,
                Items = devices.Items
                    .Select(t => t.ToDiscovererRegistration())
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<DiscovererListModel> QueryDiscoverersAsync(
            DiscovererQueryModel model, int? pageSize, CancellationToken ct) {

            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Discoverer}' ";

            if (model?.Connected != null) {
                // If flag provided, include it in search
                if (model.Connected.Value) {
                    query += $"AND connectionState = 'Connected' ";
                }
                else {
                    query += $"AND connectionState != 'Connected' ";
                }
            }

            var queryResult = await _iothub.QueryDeviceTwinsAsync(query, null,
                pageSize, ct).ConfigureAwait(false);
            return new DiscovererListModel {
                ContinuationToken = queryResult.ContinuationToken,
                Items = queryResult.Items
                    .Select(t => t.ToDiscovererRegistration())
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IJsonSerializer _serializer;
        private readonly IDirectoryEventBroker<IDiscovererRegistryListener> _broker;
        private readonly ILogger _logger;
    }
}
