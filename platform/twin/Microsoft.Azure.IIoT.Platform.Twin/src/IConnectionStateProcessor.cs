// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Connection state event processing
    /// </summary>
    public interface IConnectionStateProcessor {

        /// <summary>
        /// Handle connection state event messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnConnectionStateChangeAsync(ConnectionStateEventModel message);
    }
}
