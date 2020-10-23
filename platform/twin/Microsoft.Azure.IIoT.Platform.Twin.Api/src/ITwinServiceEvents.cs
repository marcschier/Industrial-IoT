﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api {
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin service events
    /// </summary>
    public interface ITwinServiceEvents {

        /// <summary>
        /// Subscribe to twin events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeTwinEventsAsync(
            Func<TwinEventApiModel, Task> callback);
    }
}
