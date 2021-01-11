// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Device registration model extensions
    /// </summary>
    internal static class DeviceRegistrationModelEx {

        /// <summary>
        /// Convert registration to twin
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static Twin ToTwin(this DeviceRegistrationModel registration,
            string deviceId, string moduleId) {
            return new Twin(deviceId) {
                ModuleId = moduleId,
                DeviceId = deviceId,
                Tags = registration?.Tags?.ToTwinCollection(),
                Properties = new TwinProperties {
                    Desired =
                        registration?.Properties?.ToTwinCollection(),
                }
            };
        }

        /// <summary>
        /// Convert registration to device
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static Device ToDevice(this DeviceRegistrationModel registration,
            string deviceId) {
            return new Device(deviceId) {
                Scope = registration?.DeviceScope,
                Capabilities = registration?.Capabilities?.ToDeviceCapabilities()
            };
        }

        /// <summary>
        /// Convert registration to module
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static Module ToModule(this DeviceRegistrationModel registration,
            string deviceId, string moduleId) {
            return new Module(deviceId, moduleId) {
                ManagedBy = deviceId,
            };
        }

        /// <summary>
        /// Convert registration to twin
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static TwinState ToTwinState(this DeviceRegistrationModel registration) {
            return new TwinState(
                registration?.Tags?.ToTwinCollection(),
                registration?.Properties?.ToTwinCollection());
        }

        /// <summary>
        /// Convert to enrollment group id
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="attestation"></param>
        /// <returns></returns>
        public static EnrollmentGroup ToEnrollmentGroup(
            this DeviceRegistrationModel registration, string enrollmentGroupId,
            Attestation attestation) {
            if (string.IsNullOrEmpty(enrollmentGroupId)) {
                throw new ArgumentNullException(nameof(enrollmentGroupId));
            }
            if (attestation == null) {
                throw new ArgumentNullException(nameof(attestation));
            }
            return new EnrollmentGroup(enrollmentGroupId, attestation) {
                // TODO DeviceScope = registration.DeviceScope,
                Capabilities = registration.Capabilities.ToDeviceCapabilities(),
                IotHubHostName = registration.Hub,
                IotHubs = registration.Hub != null ? new List<string> { registration.Hub } : null,
                AllocationPolicy = registration.Hub != null ? AllocationPolicy.Static : null,
                ReprovisionPolicy = new ReprovisionPolicy {
                    UpdateHubAssignment = true,
                    MigrateDeviceData = true
                },
                InitialTwinState = registration.ToTwinState()
            };
        }

        /// <summary>
        /// Convert to registration model
        /// </summary>
        /// <param name="group"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static DeviceRegistrationModel ToDeviceTwinRegistrationModel(
            this EnrollmentGroup group, IJsonSerializer serializer) {
            if (group == null) {
                return null;
            }
            if (serializer == null) {
                throw new ArgumentNullException(nameof(serializer));
            }
            return new DeviceRegistrationModel {
                Capabilities = group.Capabilities.ToCapabilitiesModel(),
                Hub = group.IotHubHostName,
                // TODO DeviceScope = group.DeviceScope,
                Tags = group.InitialTwinState?.Tags.ToTwinProperties(serializer),
                Properties = group.InitialTwinState?.DesiredProperties.ToTwinProperties(serializer)
            };
        }
    }
}
