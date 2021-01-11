// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage {
    using Microsoft.IIoT.Exceptions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Documents extensions
    /// </summary>
    public static class DocumentsEx {

        /// <summary>
        /// Gets an item.
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IDocumentInfo<T>> GetAsync<T>(
            this IDocumentCollection documents, string id, OperationOptions options = null,
            CancellationToken ct = default) {
            if (documents is null) {
                throw new System.ArgumentNullException(nameof(documents));
            }
            var result = await documents.FindAsync<T>(id, options, ct).ConfigureAwait(false);
            if (result == null) {
                throw new ResourceNotFoundException($"Resource {id} not found");
            }
            return result;
        }
    }
}
