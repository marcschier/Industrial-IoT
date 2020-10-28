// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    /// <summary>
    /// Authentication information
    /// </summary>
    public class DeviceAuthenticationModel {

        /// <summary>
        /// Primary sas key
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Secondary sas key
        /// </summary>
        public string SecondaryKey { get; set; }
    }
}
