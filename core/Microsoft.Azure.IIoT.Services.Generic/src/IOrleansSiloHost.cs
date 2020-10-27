// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Services.Orleans {
    using global::Orleans.Hosting;

    /// <summary>
    /// Orleans silo host
    /// </summary>
    public interface IOrleansSiloHost {

        /// <summary>
        /// Silo host
        /// </summary>
        ISiloHost SiloHost { get; set; }
    }
}