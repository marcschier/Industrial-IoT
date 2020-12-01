// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    /// <summary>
    /// Device registration model extensions
    /// </summary>
    public static class DeviceRegistrationModelEx {

        /// <summary>
        /// Convert registration to twin
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static Twin ToTwin(this DeviceRegistrationModel registration) {
            if (registration == null) {
                return null;
            }
            return new Twin(registration.Id) {
                ETag = "*",
                ModuleId = registration.ModuleId,
                DeviceId = registration.Id,
                Tags = registration.Tags?.ToTwinCollection(),
                Properties = new TwinProperties {
                    Desired =
                        registration.Properties?.Desired?.ToTwinCollection(),
                }
            };
        }

        /// <summary>
        /// Convert registration to device
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static Device ToDevice(this DeviceRegistrationModel registration) {
            if (registration == null) {
                return null;
            }
            return new Device(registration.Id) {
                Scope = registration.DeviceScope,
                Capabilities = registration.Capabilities?.ToCapabilities()
            };
        }

        /// <summary>
        /// Convert registration to module
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static Module ToModule(this DeviceRegistrationModel registration) {
            if (registration == null) {
                return null;
            }
            return new Module(registration.Id, registration.ModuleId) {
                ManagedBy = registration.Id,
            };
        }
    }
}
