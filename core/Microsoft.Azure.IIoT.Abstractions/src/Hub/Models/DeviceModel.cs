// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    /// <summary>
    /// Model of device registry document
    /// </summary>
    public class DeviceModel {

        /// <summary>
        /// Etag for comparison
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Device id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Module id
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Status of the device
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Authentication information
        /// </summary>
        public DeviceAuthenticationModel Authentication { get; set; }

        /// <summary>
        /// Corresponding Device's ConnectionState
        /// </summary>
        public string ConnectionState { get; set; }
    }
}
