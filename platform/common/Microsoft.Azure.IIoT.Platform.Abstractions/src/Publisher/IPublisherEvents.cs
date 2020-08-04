// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using System;

    /// <summary>
    /// Emits Publisher events
    /// </summary>
    public interface IPublisherEvents<T> where T : class {

        /// <summary>
        /// Register listener
        /// </summary>
        /// <returns></returns>
        Action Register(T listener);
    }
}
