// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher status and diagnostic services
    /// </summary>
    public interface IPublisherDiagnostics {

        /// <summary>
        /// Get publisher runtime status
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="ct"></param>
        /// <returns>Supervisor diagnostics</returns>
        Task<SupervisorStatusModel> GetPublisherStatusAsync(
            string publisherId, CancellationToken ct = default);

        /// <summary>
        /// Reset and restart supervisor
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="ct"></param>
        Task ResetPublisherAsync(string publisherId,
            CancellationToken ct = default);
    }
}
