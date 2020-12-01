// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Devices.Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Model extensions
    /// </summary>
    public static class DeviceTwinModelEx {

        /// <summary>
        /// Check whether twin is connected
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static bool? IsConnected(this DeviceTwinModel twin) {
            if (twin == null) {
                return null;
            }
            return twin.ConnectionState?.Equals("Connected",
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Check whether twin is enabled
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static bool? IsEnabled(this DeviceTwinModel twin) {
            if (twin == null) {
                return null;
            }
            return twin.Status?.Equals("enabled",
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Check whether twin is disabled
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static bool? IsDisabled(this DeviceTwinModel twin) {
            if (twin == null) {
                return null;
            }
            return twin.Status?.Equals("disabled",
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Clone twin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DeviceTwinModel Clone(this DeviceTwinModel model) {
            if (model == null) {
                return null;
            }
            return new DeviceTwinModel {
                Capabilities = model.Capabilities == null ? null :
                    new DeviceCapabilitiesModel {
                        IotEdge = model.Capabilities.IotEdge
                    },
                ConnectionState = model.ConnectionState,
                Etag = model.Etag,
                Id = model.Id,
                Hub = model.Hub,
                LastActivityTime = model.LastActivityTime,
                ModuleId = model.ModuleId,
                Properties = model.Properties.Clone(),
                Status = model.Status,
                StatusReason = model.StatusReason,
                StatusUpdatedTime = model.StatusUpdatedTime,
                Tags = model.Tags?
                    .ToDictionary(kv => kv.Key, kv => kv.Value?.Copy()),
                Version = model.Version
            };
        }

        /// <summary>
        /// Consolidated
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Dictionary<string, VariantValue> GetConsolidatedProperties(
            this DeviceTwinModel model) {
            if (model is null) {
                throw new ArgumentNullException(nameof(model));
            }

            var desired = model.Properties?.Desired;
            var reported = model.Properties?.Reported;
            if (reported == null || desired == null) {
                return (reported ?? desired)?.ToDictionary(k => k.Key, v => v.Value) ??
                    new Dictionary<string, VariantValue>();
            }

            var properties = new Dictionary<string, VariantValue>(desired);

            // Merge with reported
            foreach (var prop in reported) {
                if (properties.TryGetValue(prop.Key, out var existing)) {
                    if (VariantValueEx.IsNull(existing) || VariantValueEx.IsNull(prop.Value)) {
                        if (VariantValueEx.IsNull(existing) && VariantValueEx.IsNull(prop.Value)) {
                            continue;
                        }
                    }
                    else if (VariantValue.DeepEquals(existing, prop.Value)) {
                        continue;
                    }
                    properties[prop.Key] = prop.Value;
                }
                else {
                    properties.Add(prop.Key, prop.Value);
                }
            }
            return properties;
        }

        /// <summary>
        /// Convert twin to twin
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="isPatch"></param>
        /// <returns></returns>
        public static Twin ToTwin(this DeviceTwinModel twin, bool isPatch) {
            if (twin == null) {
                return null;
            }
            return new Twin(twin.Id) {
                ETag = twin.Etag,
                ModuleId = twin.ModuleId,
                DeviceId = twin.Id,
                Tags = twin.Tags?.ToTwinCollection(),
                Capabilities = null, // r/o
                Version = null, // r/o
                Properties = new TwinProperties {
                    Desired =
                        twin.Properties?.Desired?.ToTwinCollection(),
                    Reported = isPatch ? null :
                        twin.Properties?.Reported?.ToTwinCollection()
                }
            };
        }

        /// <summary>
        /// Convert to twin patch
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Twin ToTwin(this IReadOnlyDictionary<string, VariantValue> props) {
            return new Twin {
                Properties = new TwinProperties {
                    Desired = props?.ToTwinCollection()
                }
            };
        }

        /// <summary>
        /// Convert twin to device twin model
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="hub"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static DeviceTwinModel DeserializeTwin(this IJsonSerializer serializer,
            Twin twin, string hub) {
            if (twin == null) {
                return null;
            }
            return new DeviceTwinModel {
                Id = twin.DeviceId,
                Etag = twin.ETag,
                Hub = hub,
                ModuleId = twin.ModuleId,
                Version = twin.Version,
                ConnectionState = twin.ConnectionState?.ToString(),
                LastActivityTime = twin.LastActivityTime,
                Status = twin.Status?.ToString(),
                StatusReason = twin.StatusReason,
                StatusUpdatedTime = twin.StatusUpdatedTime,
                Tags = serializer.DeserializeTwinProperties(twin.Tags),
                Properties = new TwinPropertiesModel {
                    Desired =
                        serializer.DeserializeTwinProperties(twin.Properties?.Desired),
                    Reported =
                        serializer.DeserializeTwinProperties(twin.Properties?.Reported)
                },
                Capabilities = twin.Capabilities?.ToModel()
            };
        }

        /// <summary>
        /// Convert to twin properties model
        /// </summary>
        /// <param name="props"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static Dictionary<string, VariantValue> DeserializeTwinProperties(
            this IJsonSerializer serializer, TwinCollection props) {
            if (props == null) {
                return null;
            }
            var model = new Dictionary<string, VariantValue>();
            foreach (KeyValuePair<string, dynamic> item in props) {
                model.AddOrUpdate(item.Key, (VariantValue)serializer.FromObject(item.Value));
            }
            return model;
        }

        /// <summary>
        /// Convert to twin collection
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        internal static TwinCollection ToTwinCollection(
            this IReadOnlyDictionary<string, VariantValue> props) {
            var collection = new TwinCollection();
            foreach (var item in props) {
                collection[item.Key] = item.Value.IsListOfValues ? item.Value.Values : item.Value.Value;
            }
            return collection;
        }
    }
}
