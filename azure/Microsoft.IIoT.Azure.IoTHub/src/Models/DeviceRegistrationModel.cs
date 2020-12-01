// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.IIoT.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Register device or module in registry
    /// </summary>
    public class DeviceRegistrationModel {

        /// <summary>
        /// Hub
        /// </summary>
        public string Hub { get; set; }

        /// <summary>
        /// Device id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Sets the scope of a device
        /// </summary>
        public string DeviceScope { get; set; }

        /// <summary>
        /// Module id if the device is a module
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Tags
        /// </summary>
        public IReadOnlyDictionary<string, VariantValue> Tags { get; set; }

        /// <summary>
        /// Settings
        /// </summary>
        public TwinPropertiesModel Properties { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        public DeviceCapabilitiesModel Capabilities { get; set; }
    }
}
