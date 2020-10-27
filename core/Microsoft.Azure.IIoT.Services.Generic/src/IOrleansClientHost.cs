// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Services.Orleans {
    using global::Orleans;

    /// <summary>
    /// Orleans client host
    /// </summary>
    public interface IOrleansClientHost : IHostProcess {

        /// <summary>
        /// Client
        /// </summary>
        IClusterClient Client { get; }
    }
}