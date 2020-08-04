// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates a hosted module
    /// </summary>
    public interface IModuleHost : IDisposable {

        /// <summary>
        /// Start module host
        /// </summary>
        /// <param name="type"></param>
        /// <param name="productInfo"></param>
        /// <param name="version"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        Task StartAsync(string type,
            string productInfo, string version,
            IProcessControl control = null);

        /// <summary>
        /// Stop module host
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
