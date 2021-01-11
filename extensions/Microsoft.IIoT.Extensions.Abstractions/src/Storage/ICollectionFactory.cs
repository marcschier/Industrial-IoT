// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage {
    using System.Threading.Tasks;

    /// <summary>
    /// Injectable container
    /// </summary>
    public interface ICollectionFactory {

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="name">Name of the container</param>
        /// <returns></returns>
        Task<IDocumentCollection> OpenAsync(string name = null);
    }
}
