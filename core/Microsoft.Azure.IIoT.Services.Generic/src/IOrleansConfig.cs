// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans {

    /// <summary>
    /// Cluster configuration
    /// </summary>
    public interface IOrleansConfig {

        /// <summary>
        /// Cluster
        /// </summary>
        string ClusterId { get; }

        /// <summary>
        /// Service
        /// </summary>
        string ServiceId { get; }
    }
}
