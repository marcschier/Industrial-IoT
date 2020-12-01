// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Edge gateway registration extensions
    /// </summary>
    public static class GatewayRegistrationEx {

        /// <summary>
        /// Create device twin
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static DeviceTwinModel ToDeviceTwin(this GatewayRegistration registration) {
            return Patch(null, registration);
        }

        /// <summary>
        /// Create patch twin model to upload
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="update"></param>
        public static DeviceTwinModel Patch(this GatewayRegistration existing,
            GatewayRegistration update) {

            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();

            if (update != null) {

                // Tags

                if (update?.IsDisabled != null && update.IsDisabled != existing?.IsDisabled) {
                    tags.Add(nameof(GatewayRegistration.IsDisabled), (update?.IsDisabled ?? false) ?
                        true : (bool?)null);
                }

                if (update?.SiteId != existing?.SiteId) {
                    tags.Add(TwinProperty.SiteId, update?.SiteId);
                }
            }

            tags.Add(nameof(GatewayRegistration.DeviceType), update?.DeviceType);

            return new DeviceTwinModel {
                Etag = existing?.Etag,
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
        public static GatewayRegistration ToGatewayRegistration(this DeviceTwinModel twin,
            Dictionary<string, VariantValue> properties) {
            if (twin == null) {
                return null;
            }

            if (properties is null) {
                throw new ArgumentNullException(nameof(properties));
            }

            var registration = new GatewayRegistration {
                // Device

                Hub = twin.Hub,
                DeviceId = twin.Id,
                Etag = twin.Etag,

                // Connected
                Connected = twin.IsConnected() ?? false,

                // Tags

                IsDisabled =
                    twin.Tags.GetValueOrDefault<bool>(nameof(GatewayRegistration.IsDisabled), null),
                Type =
                    twin.Tags.GetValueOrDefault<string>(TwinProperty.Type, null),
                SiteId =
                    twin.Tags.GetValueOrDefault<string>(TwinProperty.SiteId, null),

                // Properties

            };
            return registration;
        }

        /// <summary>
        /// Get supervisor registration from twin
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static GatewayRegistration ToGatewayRegistration(this DeviceTwinModel twin) {
            return ToGatewayRegistration(twin, out _);
        }

        /// <summary>
        /// Make sure to get the registration information from the right place.
        /// Reported (truth) properties take precedence over desired. However,
        /// if there is nothing reported, it means the endpoint is not currently
        /// serviced, thus we use desired as if they are attributes of the
        /// endpoint.
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="connected"></param>
        /// <returns></returns>
        public static GatewayRegistration ToGatewayRegistration(this DeviceTwinModel twin,
            out bool connected) {

            if (twin == null) {
                connected = false;
                return null;
            }
            if (twin.Tags == null) {
                twin.Tags = new Dictionary<string, VariantValue>();
            }

            var consolidated =
                ToGatewayRegistration(twin, twin.GetConsolidatedProperties());
            connected = consolidated.Connected;
            return consolidated;
        }

        /// <summary>
        /// Patch this registration and create patch twin model to upload
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        public static GatewayRegistration ToGatewayRegistration(
            this GatewayModel model, bool? disabled = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            var deviceId = model.Id;
            return new GatewayRegistration {
                IsDisabled = disabled,
                DeviceId = deviceId,
                Connected = model.Connected ?? false,
                SiteId = model.SiteId,
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static GatewayModel ToServiceModel(this GatewayRegistration registration) {
            if (registration == null) {
                return null;
            }
            return new GatewayModel {
                Id = registration.DeviceId,
                SiteId = registration.SiteId,
                Connected = registration.IsConnected() ? true : (bool?)null
            };
        }
    }
}
