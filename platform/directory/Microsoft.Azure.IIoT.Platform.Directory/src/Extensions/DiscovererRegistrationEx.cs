// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Discoverer registration extensions
    /// </summary>
    public static class DiscovererRegistrationEx {

        /// <summary>
        /// Create device twin
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static DeviceTwinModel ToDeviceTwin(
            this DiscovererRegistration registration, IJsonSerializer serializer) {
            return Patch(null, registration, serializer);
        }

        /// <summary>
        /// Create patch twin model to upload
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="update"></param>
        /// <param name="serializer"></param>
        public static DeviceTwinModel Patch(this DiscovererRegistration existing,
            DiscovererRegistration update, IJsonSerializer serializer) {
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }

            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();

            // Tags
            if (update != null) {

                if (update?.IsDisabled != null && update.IsDisabled != existing?.IsDisabled) {
                    tags.Add(nameof(DiscovererRegistration.IsDisabled), (update?.IsDisabled ?? false) ?
                        true : (bool?)null);
                    tags.Add(nameof(DiscovererRegistration.NotSeenSince), (update?.IsDisabled ?? false) ?
                        DateTime.UtcNow : (DateTime?)null);
                }

                // Settings

                if (update?.LogLevel != existing?.LogLevel) {
                    properties.Add(nameof(DiscovererRegistration.LogLevel),
                        update?.LogLevel == null ?
                        null : serializer.FromObject(update.LogLevel.ToString()));
                }
            }
            tags.Add(nameof(DiscovererRegistration.DeviceType), update?.DeviceType);

            return new DeviceTwinModel {
                Etag = existing?.Etag,
                ModuleId = update?.ModuleId ?? existing?.ModuleId,
                Id = update?.DeviceId ?? existing?.DeviceId,
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
        public static DiscovererRegistration ToDiscovererRegistration(this DeviceTwinModel twin,
            IReadOnlyDictionary<string, VariantValue> properties) {
            if (twin == null) {
                return null;
            }

            var tags = twin.Tags ?? new Dictionary<string, VariantValue>();

            var registration = new DiscovererRegistration {
                // Device

                Hub = twin.Hub,
                DeviceId = twin.Id,
                ModuleId = twin.ModuleId,
                Etag = twin.Etag,
                Connected = twin.IsConnected() ?? false,

                // Tags

                IsDisabled =
                    tags.GetValueOrDefault<bool>(nameof(DiscovererRegistration.IsDisabled), null),
                NotSeenSince =
                    tags.GetValueOrDefault<DateTime>(nameof(DiscovererRegistration.NotSeenSince), null),

                // Properties

                LogLevel =
                    properties.GetValueOrDefault<TraceLogLevel>(nameof(DiscovererRegistration.LogLevel), null),

                Version =
                    properties.GetValueOrDefault<string>(TwinProperty.Version, null),
                Type =
                    properties.GetValueOrDefault<string>(TwinProperty.Type, null)
            };
            return registration;
        }

        /// <summary>
        /// Get discoverer registration from twin
        /// </summary>
        /// <param name="onlyServerState"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static DiscovererRegistration ToDiscovererRegistration(this DeviceTwinModel twin, bool onlyServerState = false) {
            return ToDiscovererRegistration(twin, onlyServerState, out _);
        }

        /// <summary>
        /// Make sure to get the registration information from the right place.
        /// Reported (truth) properties take precedence over desired. However,
        /// if there is nothing reported, it means the endpoint is not currently
        /// serviced, thus we use desired as if they are attributes of the
        /// endpoint.
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="onlyServerState"></param>
        /// <param name="connected"></param>
        /// <returns></returns>
        public static DiscovererRegistration ToDiscovererRegistration(this DeviceTwinModel twin,
             bool onlyServerState, out bool connected) {

            if (twin == null) {
                connected = false;
                return null;
            }
            if (twin.Tags == null) {
                twin.Tags = new Dictionary<string, VariantValue>();
            }

            var consolidated =
                ToDiscovererRegistration(twin, twin.GetConsolidatedProperties());
            var desired = (twin.Properties?.Desired == null) ? null :
                ToDiscovererRegistration(twin, twin.Properties.Desired);

            connected = consolidated.Connected;
            if (desired != null) {
                desired.Connected = connected;
                if (desired.LogLevel == null && consolidated.LogLevel != null) {
                    // Not set by user, but reported, so set as desired
                    desired.LogLevel = consolidated.LogLevel;
                }
                desired.Version = consolidated.Version;
            }

            if (onlyServerState) {
                consolidated = desired;
            }

            consolidated._isInSync = consolidated.IsInSyncWith(desired);
            consolidated._desired = desired;
            return consolidated;
        }

        /// <summary>
        /// Patch this registration and create patch twin model to upload
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        public static DiscovererRegistration ToDiscovererRegistration(
            this DiscovererModel model, bool? disabled = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            var deviceId = HubResource.Parse(model.Id, out var hub,
                out var moduleId);
            return new DiscovererRegistration {
                IsDisabled = disabled,
                DeviceId = deviceId,
                ModuleId = moduleId,
                Hub = hub,
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
        public static DiscovererModel ToServiceModel(this DiscovererRegistration registration) {
            if (registration == null) {
                return null;
            }
            return new DiscovererModel {
                Id = HubResource.Format(registration.Hub,
                    registration.DeviceId, registration.ModuleId),
                Version = registration.Version,
                LogLevel = registration.LogLevel,
                Connected = registration.IsConnected() ? true : (bool?)null,
                OutOfSync = registration.IsConnected() && !registration._isInSync ? true : (bool?)null
            };
        }

        /// <summary>
        /// Flag twin as synchronized - i.e. it matches the other.
        /// </summary>
        /// <param name="reported"></param>
        /// <param name="desired"></param>
        internal static bool IsInSyncWith(this DiscovererRegistration reported,
            DiscovererRegistration desired) {
            if (reported == null) {
                return desired == null;
            }
            return
                desired != null &&
                reported.LogLevel == desired.LogLevel;
        }
    }
}
