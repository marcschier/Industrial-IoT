// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices.Shared;

    /// <summary>
    /// Device capabilities model extensions
    /// </summary>
    public static class CapabilitiesModelEx {

        /// <summary>
        /// Convert capabilities model to capabilities
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DeviceCapabilities ToDeviceCapabilities(
            this CapabilitiesModel model) {
            if (model == null) {
                return null;
            }
            return new DeviceCapabilities {
                IotEdge = model.IotEdge ?? false
            };
        }

        /// <summary>
        /// Convert capabilities to model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static CapabilitiesModel ToCapabilitiesModel(
            this DeviceCapabilities model) {
            if (model == null) {
                return null;
            }
            return new CapabilitiesModel {
                IotEdge = model.IotEdge
            };
        }
    }
}
