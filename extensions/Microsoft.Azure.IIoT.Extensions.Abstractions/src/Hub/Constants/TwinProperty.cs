// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub {

    /// <summary>
    /// Common IIoT module and device twin properties
    /// </summary>
    public static class TwinProperty {

        /// <summary>
        /// Type property name constant
        /// </summary>
        public const string Type = "__type__";

        /// <summary>
        /// Site id property name constant
        /// </summary>
        public const string SiteId = "__siteid__";

        /// <summary>
        /// Version property name constant
        /// </summary>
        public const string Version = "__version__";

        /// <summary>
        /// Service endpoint url
        /// </summary>
        public const string ServiceEndpoint = nameof(ServiceEndpoint);
    }
}
