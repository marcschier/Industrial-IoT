// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {

    /// <summary>
    /// Model of device registration
    /// </summary>
    public class DeviceRegistrationStateModel {

        /// <summary>
        /// Hub
        /// </summary>
        public string Hub { get; set; }

        /// <summary>
        /// Device id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Registration
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Enrollment
        /// </summary>
        public string EnrollmentGroupId { get; set; }

        /// <summary>
        /// Status of the device
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Etag
        /// </summary>
        public string Etag { get; set; }
    }
}
