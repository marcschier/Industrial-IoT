﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using System;

    /// <summary>
    /// Emits Registry events
    /// </summary>
    public interface IDiscoveryEvents<T> where T : class {

        /// <summary>
        /// Register listener
        /// </summary>
        /// <returns></returns>
        Action Register(T listener);
    }
}
