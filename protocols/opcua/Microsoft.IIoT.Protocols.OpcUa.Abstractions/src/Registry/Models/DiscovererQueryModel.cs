// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Discoverer registration query request
    /// </summary>
    public class DiscovererQueryModel {

        /// <summary>
        /// Included connected or disconnected
        /// </summary>
        public bool? Connected { get; set; }
    }
}
