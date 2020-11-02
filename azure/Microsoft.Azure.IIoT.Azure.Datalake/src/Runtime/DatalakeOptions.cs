// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Datalake {

    /// <summary>
    /// Datalake file storage configuration
    /// </summary>
    public class DatalakeOptions {

        /// <summary>
        /// Storage endpoint
        /// </summary>
        public string EndpointSuffix { get; set; }

        /// <summary>
        /// Storage account
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Storage account key
        /// </summary>
        public string AccountKey { get; set; }
    }
}
