// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
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
