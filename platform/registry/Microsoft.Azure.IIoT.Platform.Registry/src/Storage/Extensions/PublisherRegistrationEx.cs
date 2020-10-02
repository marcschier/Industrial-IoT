// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Publisher registration extensions
    /// </summary>
    public static class PublisherRegistrationEx {

        /// <summary>
        /// Create device twin
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static DeviceTwinModel ToDeviceTwin(
            this PublisherRegistration registration, IJsonSerializer serializer) {
            return Patch(null, registration, serializer);
        }

        /// <summary>
        /// Create patch twin model to upload
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="update"></param>
        /// <param name="serializer"></param>
        public static DeviceTwinModel Patch(this PublisherRegistration existing,
            PublisherRegistration update, IJsonSerializer serializer) {
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }

            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();

            if (update != null) {
                // Tags

                if (update?.IsDisabled != null && update.IsDisabled != existing?.IsDisabled) {
                    tags.Add(nameof(PublisherRegistration.IsDisabled), (update?.IsDisabled ?? false) ?
                        true : (bool?)null);
                    tags.Add(nameof(PublisherRegistration.NotSeenSince), (update?.IsDisabled ?? false) ?
                        DateTime.UtcNow : (DateTime?)null);
                }

                // Settings

                if (update?.LogLevel != existing?.LogLevel) {
                    properties.Add(nameof(PublisherRegistration.LogLevel),
                        update?.LogLevel == null ?
                        null : serializer.FromObject(update.LogLevel.ToString()));
                }
            }


            tags.Add(nameof(GatewayRegistration.DeviceType), update?.DeviceType);

            return new DeviceTwinModel {
                Etag = existing?.Etag,
                Id = update?.DeviceId ?? existing?.DeviceId,
                ModuleId = update?.ModuleId ?? existing?.ModuleId,
                Tags = tags,
                Properties = new TwinPropertiesModel {
                    Desired = properties
                }
            };
        }

        /// <summary>
        /// Decode tags and property into registration object
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static PublisherRegistration ToPublisherRegistration(this DeviceTwinModel twin,
            IReadOnlyDictionary<string, VariantValue> properties) {
            if (twin == null) {
                return null;
            }

            var tags = twin.Tags ?? new Dictionary<string, VariantValue>();

            var registration = new PublisherRegistration {
                // Device

                Hub = twin.Hub,
                DeviceId = twin.Id,
                ModuleId = twin.ModuleId,
                Etag = twin.Etag,
                Connected = twin.IsConnected() ?? false,

                // Tags

                IsDisabled =
                    tags.GetValueOrDefault<bool>(nameof(PublisherRegistration.IsDisabled), null),
                NotSeenSince =
                    tags.GetValueOrDefault<DateTime>(nameof(PublisherRegistration.NotSeenSince), null),

                // Properties

                LogLevel =
                    properties.GetValueOrDefault<TraceLogLevel>(nameof(PublisherRegistration.LogLevel), null),

                Version =
                    properties.GetValueOrDefault<string>(TwinProperty.Version, null),
                Type =
                    properties.GetValueOrDefault<string>(TwinProperty.Type, null)
            };
            return registration;
        }

        /// <summary>
        /// Get supervisor registration from twin
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="onlyServerState"></param>
        /// <returns></returns>
        public static PublisherRegistration ToPublisherRegistration(this DeviceTwinModel twin,
            bool onlyServerState) {
            return ToPublisherRegistration(twin, onlyServerState, out _);
        }

        /// <summary>
        /// Make sure to get the registration information from the right place.
        /// Reported (truth) properties take precedence over desired. However,
        /// if there is nothing reported, it means the endpoint is not currently
        /// serviced, thus we use desired as if they are attributes of the
        /// endpoint.
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="onlyServerState">Only desired endpoint should be returned
        /// this means that you will look at stale information.</param>
        /// <param name="connected"></param>
        /// <returns></returns>
        public static PublisherRegistration ToPublisherRegistration(this DeviceTwinModel twin,
            bool onlyServerState, out bool connected) {

            if (twin == null) {
                connected = false;
                return null;
            }
            if (twin.Tags == null) {
                twin.Tags = new Dictionary<string, VariantValue>();
            }

            var consolidated =
                ToPublisherRegistration(twin, twin.GetConsolidatedProperties());
            var desired = (twin.Properties?.Desired == null) ? null :
                ToPublisherRegistration(twin, twin.Properties.Desired);

            connected = consolidated.Connected;
            if (desired != null) {
                desired.Connected = connected;
                if (desired.LogLevel == null && consolidated.LogLevel != null) {
                    // Not set by user, but reported, so set as desired
                    desired.LogLevel = consolidated.LogLevel;
                }
                desired.Version = consolidated.Version;
            }

            if (!onlyServerState) {
                consolidated._isInSync = consolidated.IsInSyncWith(desired);
                return consolidated;
            }
            if (desired != null) {
                desired._isInSync = desired.IsInSyncWith(consolidated);
            }
            return desired;
        }

        /// <summary>
        /// Patch this registration and create patch twin model to upload
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        public static PublisherRegistration ToPublisherRegistration(
            this PublisherModel model, bool? disabled = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            var deviceId = HubResource.Parse(model.Id, out var hub,
                out var moduleId);
            return new PublisherRegistration {
                IsDisabled = disabled,
                DeviceId = deviceId,
                Hub = hub,
                ModuleId = moduleId,
                LogLevel = model.LogLevel,
                Connected = model.Connected ?? false,
                Version = null
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static PublisherModel ToServiceModel(this PublisherRegistration registration) {
            if (registration == null) {
                return null;
            }
            return new PublisherModel {
                Id = HubResource.Format(registration.Hub, registration.DeviceId,
                    registration.ModuleId),
                LogLevel = registration.LogLevel,
                Version = registration.Version,
                Connected = registration.IsConnected() ? true : (bool?)null,
                OutOfSync = registration.IsConnected() && !registration._isInSync ? true : (bool?)null
            };
        }

        /// <summary>
        /// Flag twin as synchronized - i.e. it matches the other.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="other"></param>
        internal static bool IsInSyncWith(this PublisherRegistration registration,
            PublisherRegistration other) {
            if (registration == null) {
                return other == null;
            }
            return
                other != null &&
                registration.LogLevel == other.LogLevel;
        }
    }
}
