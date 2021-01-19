﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery process processing
    /// </summary>
    public interface IDiscovererProgressProcessor {

        /// <summary>
        /// Handle discovery progress messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnDiscoveryProgressAsync(DiscoveryProgressModel message);
    }
}