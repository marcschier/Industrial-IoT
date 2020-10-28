// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore {
    using Microsoft.AspNetCore.Http;
    using System;

    /// <summary>
    /// Server
    /// </summary>
    public interface IAppServer : IDisposable {

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="request"></param>
        void Start(IServiceProvider provider, RequestDelegate request);
    }
}
