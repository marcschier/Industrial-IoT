// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Enables retrieving status
    /// </summary>
    public interface ISupervisorServices {

        /// <summary>
        /// Get supervisor status
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<SupervisorStatusModel> GetStatusAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Detach inactive entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task DetachAsync(string entityId);

        /// <summary>
        /// Attach entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        Task AttachAsync(string entityId, string secret);

        /// <summary>
        /// Reset supervisor
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ResetAsync(CancellationToken ct = default);
    }
}
