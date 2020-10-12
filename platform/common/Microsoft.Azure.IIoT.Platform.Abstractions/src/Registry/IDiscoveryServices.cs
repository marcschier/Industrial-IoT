// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Application discovery services
    /// </summary>
    public interface IDiscoveryServices {

        /// <summary>
        /// Register server from discovery url.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RegisterAsync(ServerRegistrationRequestModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Kick of an application discovery
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DiscoverAsync(DiscoveryRequestModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task CancelAsync(DiscoveryCancelModel request,
            CancellationToken ct = default);
    }
}
