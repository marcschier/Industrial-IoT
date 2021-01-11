// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {

    /// <summary>
    /// Enrollment group
    /// </summary>
    public class EnrollmentGroupModel {

        /// <summary>
        /// Identifier
        /// </summary>
        public string EnrollmentGroupId { get; set; }

        /// <summary>
        /// Device registration
        /// </summary>
        public DeviceRegistrationModel Registration { get; set; }

        /// <summary>
        /// Etag
        /// </summary>
        public string Etag { get; internal set; }
    }
}