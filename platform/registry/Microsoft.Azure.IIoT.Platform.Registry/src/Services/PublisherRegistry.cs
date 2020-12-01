// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Publisher registry which uses the IoT Hub twin services for publisher
    /// and writer group identity management.
    /// </summary>
    public sealed class PublisherRegistry : IPublisherRegistry {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="broker"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public PublisherRegistry(IDeviceTwinServices iothub, IJsonSerializer serializer,
            IRegistryEventBroker<IPublisherRegistryListener> broker, ILogger logger) {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task<PublisherModel> GetPublisherAsync(string id,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            var deviceId = HubResource.Parse(id, out var hub, out var moduleId);
            var device = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            var registration = device.ToEntityRegistration()
                as PublisherRegistration;
            if (registration == null) {
                throw new ResourceNotFoundException(
                    $"{id} is not a publisher registration.");
            }
            return registration.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdatePublisherAsync(string publisherId,
            PublisherUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(publisherId)) {
                throw new ArgumentNullException(nameof(publisherId));
            }

            // Get existing endpoint and compare to see if we need to patch.
            var deviceId = HubResource.Parse(publisherId, out var hub, out var moduleId);

            while (true) {
                try {
                    var twin = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
                    if (twin.Id != deviceId && twin.ModuleId != moduleId) {
                        throw new ArgumentException("Id must be same as twin to patch",
                            nameof(publisherId));
                    }

                    var registration = twin.ToEntityRegistration(true) as PublisherRegistration;
                    if (registration == null) {
                        throw new ResourceNotFoundException(
                            $"{publisherId} is not a publisher registration.");
                    }

                    // Update registration from update request
                    var patched = registration.ToServiceModel();

                    if (request.LogLevel != null) {
                        patched.LogLevel = request.LogLevel == TraceLogLevel.Information ?
                            null : request.LogLevel;
                    }

                    // Patch
                    twin = await _iothub.PatchAsync(registration.Patch(
                        patched.ToPublisherRegistration(), _serializer), false, ct).ConfigureAwait(false);

                    // Send update to through broker
                    registration = twin.ToEntityRegistration(true) as PublisherRegistration;
                    await _broker.NotifyAllAsync(l => l.OnPublisherUpdatedAsync(null,
                        registration.ToServiceModel())).ConfigureAwait(false);
                    return;
                }
                catch (ResourceOutOfDateException ex) {
                    _logger.LogDebug(ex, "Retrying updating publisher...");
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> ListPublishersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Publisher}' ";
            var devices = await _iothub.QueryDeviceTwinsAsync(continuation == null ? query : null,
                continuation, pageSize, ct).ConfigureAwait(false);
            return new PublisherListModel {
                ContinuationToken = devices.ContinuationToken,
                Items = devices.Items
                    .Select(t => t.ToPublisherRegistration(true))
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> QueryPublishersAsync(
            PublisherQueryModel model, int? pageSize, CancellationToken ct) {

            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Publisher}'";

            if (model?.Connected != null) {
                // If flag provided, include it in search
                if (model.Connected.Value) {
                    query += $"AND connectionState = 'Connected' ";
                }
                else {
                    query += $"AND connectionState != 'Connected' ";
                }
            }

            var queryResult = await _iothub.QueryDeviceTwinsAsync(query, null, pageSize, ct).ConfigureAwait(false);
            return new PublisherListModel {
                ContinuationToken = queryResult.ContinuationToken,
                Items = queryResult.Items
                    .Select(t => t.ToPublisherRegistration(true))
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IJsonSerializer _serializer;
        private readonly IRegistryEventBroker<IPublisherRegistryListener> _broker;
        private readonly ILogger _logger;
    }
}
