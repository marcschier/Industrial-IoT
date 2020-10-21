// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows update of connection state
    /// </summary>
    public interface IConnectionStateUpdater {

        /// <summary>
        /// Update connection state
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateConnectionStateAsync(string connectionId,
            ConnectionStateModel state, OperationContextModel context = null,
            CancellationToken ct = default);
    }
}