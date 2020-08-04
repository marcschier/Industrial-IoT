// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using System.Threading.Tasks;

    /// <summary>
    /// Bulk publish
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBulkPublishInitiator<T> {

        /// <summary>
        /// Publish everything on endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task PublishAsync(T endpoint);
    }
}
