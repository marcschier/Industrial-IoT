// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manage identities
    /// </summary>
    public interface IModuleHostManager {

        /// <summary>
        /// Currently connected or disconnected hosts
        /// </summary>
        IEnumerable<(string, bool)> Hosts { get; }

        /// <summary>
        /// Create instance for identity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secret"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task StartAsync(string id, string secret,
            CancellationToken ct = default);

        /// <summary>
        /// Delete instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task StopAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Create identity but do not wait
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        Task QueueStartAsync(string id, string secret);

        /// <summary>
        /// Delete instance but do not wait
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task QueueStopAsync(string id);
    }
}