// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
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


                var urlUpdate = update?.DiscoveryUrls.DecodeAsList().SequenceEqualsSafe(
                    existing?.DiscoveryUrls?.DecodeAsList());
                if (!(urlUpdate ?? true)) {
                    properties.Add(nameof(DiscovererRegistration.DiscoveryUrls),
                        update?.DiscoveryUrls == null ?
                        null : serializer.FromObject(update.DiscoveryUrls));
                }

                var localesUpdate = update?.Locales?.DecodeAsList()?.SequenceEqualsSafe(
                    existing?.Locales?.DecodeAsList());
                if (!(localesUpdate ?? true)) {
                    properties.Add(nameof(DiscovererRegistration.Locales),
                        update?.Locales == null ?
                        null : serializer.FromObject(update.Locales));
                }

                if (update?.Discovery != existing?.Discovery) {
                    properties.Add(nameof(DiscovererRegistration.Discovery),
                        serializer.FromObject(update?.Discovery.ToString()));
                }

                if (update?.AddressRangesToScan != existing?.AddressRangesToScan) {
                    properties.Add(nameof(DiscovererRegistration.AddressRangesToScan),
                        update?.AddressRangesToScan);
                }

                if (update?.NetworkProbeTimeout != existing?.NetworkProbeTimeout) {
                    properties.Add(nameof(DiscovererRegistration.NetworkProbeTimeout),
                        update?.NetworkProbeTimeout);
                }

                if (update?.LogLevel != existing?.LogLevel) {
                    properties.Add(nameof(DiscovererRegistration.LogLevel),
                        update?.LogLevel == null ?
                        null : serializer.FromObject(update.LogLevel.ToString()));
                }

                if (update?.MaxNetworkProbes != existing?.MaxNetworkProbes) {
                    properties.Add(nameof(DiscovererRegistration.MaxNetworkProbes),
                        update?.MaxNetworkProbes);
                }

                if (update?.PortRangesToScan != existing?.PortRangesToScan) {
                    properties.Add(nameof(DiscovererRegistration.PortRangesToScan),
                        update?.PortRangesToScan);
                }

                if (update?.PortProbeTimeout != existing?.PortProbeTimeout) {
                    properties.Add(nameof(DiscovererRegistration.PortProbeTimeout),
                        update?.PortProbeTimeout);
                }

                if (update?.MaxPortProbes != existing?.MaxPortProbes) {
                    properties.Add(nameof(DiscovererRegistration.MaxPortProbes),
                        update?.MaxPortProbes);
                }

                if (update?.IdleTimeBetweenScans != existing?.IdleTimeBetweenScans) {
                    properties.Add(nameof(DiscovererRegistration.IdleTimeBetweenScans),
                        update?.IdleTimeBetweenScans);
                }

                if (update?.MinPortProbesPercent != existing?.MinPortProbesPercent) {
                    properties.Add(nameof(DiscovererRegistration.MinPortProbesPercent),
                        update?.MinPortProbesPercent);
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
                Discovery =
                    properties.GetValueOrDefault(nameof(DiscovererRegistration.Discovery), DiscoveryMode.Off),
                AddressRangesToScan =
                    properties.GetValueOrDefault<string>(nameof(DiscovererRegistration.AddressRangesToScan), null),
                NetworkProbeTimeout =
                    properties.GetValueOrDefault<TimeSpan>(nameof(DiscovererRegistration.NetworkProbeTimeout), null),
                MaxNetworkProbes =
                    properties.GetValueOrDefault<int>(nameof(DiscovererRegistration.MaxNetworkProbes), null),
                PortRangesToScan =
                    properties.GetValueOrDefault<string>(nameof(DiscovererRegistration.PortRangesToScan), null),
                PortProbeTimeout =
                    properties.GetValueOrDefault<TimeSpan>(nameof(DiscovererRegistration.PortProbeTimeout), null),
                MaxPortProbes =
                    properties.GetValueOrDefault<int>(nameof(DiscovererRegistration.MaxPortProbes), null),
                MinPortProbesPercent =
                    properties.GetValueOrDefault<int>(nameof(DiscovererRegistration.MinPortProbesPercent), null),
                IdleTimeBetweenScans =
                    properties.GetValueOrDefault<TimeSpan>(nameof(DiscovererRegistration.IdleTimeBetweenScans), null),
                DiscoveryUrls =
                    properties.GetValueOrDefault<Dictionary<string, string>>(nameof(DiscovererRegistration.DiscoveryUrls), null),
                Locales =
                    properties.GetValueOrDefault<Dictionary<string, string>>(nameof(DiscovererRegistration.Locales), null),

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
                Discovery = model.RequestedMode ?? DiscoveryMode.Off,
                AddressRangesToScan = model.RequestedConfig?.AddressRangesToScan,
                NetworkProbeTimeout = model.RequestedConfig?.NetworkProbeTimeout,
                MaxNetworkProbes = model.RequestedConfig?.MaxNetworkProbes,
                PortRangesToScan = model.RequestedConfig?.PortRangesToScan,
                PortProbeTimeout = model.RequestedConfig?.PortProbeTimeout,
                MaxPortProbes = model.RequestedConfig?.MaxPortProbes,
                IdleTimeBetweenScans = model.RequestedConfig?.IdleTimeBetweenScans,
                MinPortProbesPercent = model.RequestedConfig?.MinPortProbesPercent,
                DiscoveryUrls = model.RequestedConfig?.DiscoveryUrls?.
                    EncodeAsDictionary(),
                Locales = model.RequestedConfig?.Locales?.
                    EncodeAsDictionary(),
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
                Discovery = registration.Discovery != DiscoveryMode.Off ?
                    registration.Discovery : (DiscoveryMode?)null,
                Id = HubResource.Format(registration.Hub,
                    registration.DeviceId, registration.ModuleId),
                Version = registration.Version,
                LogLevel = registration.LogLevel,
                DiscoveryConfig = registration.ToConfigModel(),
                RequestedMode = registration._desired?.Discovery != DiscoveryMode.Off ?
                    registration._desired?.Discovery : null,
                RequestedConfig = registration._desired.ToConfigModel(),
                Connected = registration.IsConnected() ? true : (bool?)null,
                OutOfSync = registration.IsConnected() && !registration._isInSync ? true : (bool?)null
            };
        }

        /// <summary>
        /// Returns if no discovery config specified
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        private static bool IsNullConfig(this DiscovererRegistration registration) {
            if (registration == null) {
                return true;
            }
            if (string.IsNullOrEmpty(registration.AddressRangesToScan) &&
                string.IsNullOrEmpty(registration.PortRangesToScan) &&
                registration.MaxNetworkProbes == null &&
                registration.NetworkProbeTimeout == null &&
                registration.MaxPortProbes == null &&
                registration.MinPortProbesPercent == null &&
                registration.PortProbeTimeout == null &&
                (registration.DiscoveryUrls == null || registration.DiscoveryUrls.Count == 0) &&
                (registration.Locales == null || registration.Locales.Count == 0) &&
                registration.IdleTimeBetweenScans == null) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns config model
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        private static DiscoveryConfigModel ToConfigModel(this DiscovererRegistration registration) {
            return registration.IsNullConfig() ? null : new DiscoveryConfigModel {
                AddressRangesToScan = string.IsNullOrEmpty(registration.AddressRangesToScan) ?
                    null : registration.AddressRangesToScan,
                PortRangesToScan = string.IsNullOrEmpty(registration.PortRangesToScan) ?
                    null : registration.PortRangesToScan,
                MaxNetworkProbes = registration.MaxNetworkProbes,
                NetworkProbeTimeout = registration.NetworkProbeTimeout,
                MaxPortProbes = registration.MaxPortProbes,
                MinPortProbesPercent = registration.MinPortProbesPercent,
                PortProbeTimeout = registration.PortProbeTimeout,
                IdleTimeBetweenScans = registration.IdleTimeBetweenScans,
                DiscoveryUrls = registration.DiscoveryUrls?.DecodeAsList(),
                Locales = registration.Locales?.DecodeAsList()
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
                reported.LogLevel == desired.LogLevel &&
                reported.Discovery == desired.Discovery &&
                (string.IsNullOrEmpty(desired.AddressRangesToScan) ||
                    reported.AddressRangesToScan == desired.AddressRangesToScan) &&
                (string.IsNullOrEmpty(desired.PortRangesToScan) ||
                    reported.PortRangesToScan == desired.PortRangesToScan) &&
                (desired.MaxNetworkProbes == null ||
                    reported.MaxNetworkProbes == desired.MaxNetworkProbes) &&
                (desired.MaxNetworkProbes == null ||
                    reported.NetworkProbeTimeout == desired.NetworkProbeTimeout) &&
                (desired.MaxPortProbes == null ||
                    reported.MaxPortProbes == desired.MaxPortProbes) &&
                (desired.MinPortProbesPercent == null ||
                    reported.MinPortProbesPercent == desired.MinPortProbesPercent) &&
                (desired.PortProbeTimeout == null ||
                    reported.PortProbeTimeout == desired.PortProbeTimeout) &&
                (desired.IdleTimeBetweenScans == null ||
                    reported.IdleTimeBetweenScans == desired.IdleTimeBetweenScans) &&
                ((desired.DiscoveryUrls.DecodeAsList()?.Count ?? 0) == 0 ||
                    reported.DiscoveryUrls.DecodeAsList().SequenceEqualsSafe(
                        desired.DiscoveryUrls.DecodeAsList())) &&
                ((desired.Locales.DecodeAsList()?.Count ?? 0) == 0 ||
                    reported.Locales.DecodeAsList().SequenceEqualsSafe(
                        desired.Locales.DecodeAsList()));
        }
    }
}
