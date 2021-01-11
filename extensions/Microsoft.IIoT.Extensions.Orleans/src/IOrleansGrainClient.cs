// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans {
    using global::Orleans;

    /// <summary>
    /// Orleans grain client
    /// </summary>
    public interface IOrleansGrainClient {

        /// <summary>
        /// Client
        /// </summary>
        IGrainFactory Grains { get; }
    }
}