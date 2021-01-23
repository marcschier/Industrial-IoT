// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows update of connection state
    /// </summary>
    public interface ITwinStateUpdater {

        /// <summary>
        /// Update connection state
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateConnectionStateAsync(string twinId,
            ConnectionStateModel state, OperationContextModel context = null,
            CancellationToken ct = default);
    }
}