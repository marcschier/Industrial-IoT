// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Twin {
    using System;

    /// <summary>
    /// Emits Twin events
    /// </summary>
    public interface ITwinEvents<T> where T : class {

        /// <summary>
        /// Register listener
        /// </summary>
        /// <returns></returns>
        Action Register(T listener);
    }
}
