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
        /// <param name="version"></param>
        /// <returns></returns>
        Task StartAsync(string type, string version);

        /// <summary>
        /// Stop module host
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
