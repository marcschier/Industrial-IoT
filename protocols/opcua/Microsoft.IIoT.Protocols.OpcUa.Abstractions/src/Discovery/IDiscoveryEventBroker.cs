﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry event broker
    /// </summary>
    public interface IDiscoveryEventBroker<T> where T : class {

        /// <summary>
        /// Notify all listeners
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        Task NotifyAllAsync(Func<T, Task> evt);
    }
}
