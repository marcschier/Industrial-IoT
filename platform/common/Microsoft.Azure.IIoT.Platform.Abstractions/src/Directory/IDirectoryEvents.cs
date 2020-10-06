// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Directory {
    using System;

    /// <summary>
    /// Emits Registry events
    /// </summary>
    public interface IDirectoryEvents<T> where T : class {

        /// <summary>
        /// Register listener
        /// </summary>
        /// <returns></returns>
        Action Register(T listener);
    }
}
