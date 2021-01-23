// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Api {

    /// <summary>
    /// Configuration for service
    /// </summary>
    public interface IDiscoveryConfig {

        /// <summary>
        /// Discovery service url
        /// </summary>
        string DiscoveryServiceUrl { get; }
    }
}
