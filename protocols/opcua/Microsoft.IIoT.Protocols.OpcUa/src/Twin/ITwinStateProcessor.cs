// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin state event processing
    /// </summary>
    public interface ITwinStateProcessor {

        /// <summary>
        /// Handle twin state event messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnTwinStateChangeAsync(TwinStateEventModel message);
    }
}
