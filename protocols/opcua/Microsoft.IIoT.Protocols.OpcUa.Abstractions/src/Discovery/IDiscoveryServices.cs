// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
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
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RegisterAsync(ServerRegistrationRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Kick of an application discovery
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DiscoverAsync(DiscoveryRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task CancelAsync(DiscoveryCancelModel request,
           OperationContextModel context = null,
           CancellationToken ct = default);
    }
}
