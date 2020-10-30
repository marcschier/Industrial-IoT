// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans {
    /// <summary>
    /// Event Bus configuration
    /// </summary>
    public interface IOrleansBusConfig {

        /// <summary>
        /// Prefix
        /// </summary>
        string Prefix { get; }
    }
}
