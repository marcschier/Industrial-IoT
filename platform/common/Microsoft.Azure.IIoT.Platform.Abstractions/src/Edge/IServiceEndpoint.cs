// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge {
    using System;

    /// <summary>
    /// Service endpoint provider
    /// </summary>
    public interface IServiceEndpoint {

        /// <summary>
        /// Service endpoint
        /// </summary>
        string ServiceEndpoint { get; }

        /// <summary>
        /// Configuration change events
        /// </summary>
        event EventHandler OnServiceEndpointUpdated;
    }
}