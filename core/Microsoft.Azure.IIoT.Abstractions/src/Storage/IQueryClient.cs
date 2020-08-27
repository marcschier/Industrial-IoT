// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage {
    using System;

    /// <summary>
    /// Provides Query capability
    /// </summary>
    public interface IQueryClient {

        /// <summary>
        /// Create Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQuery<T> CreateQuery<T>(int? pageSize = null,
            OperationOptions options = null);

        /// <summary>
        /// Continue a previously run query using continuation token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="continuationToken"></param>
        /// <param name="pageSize"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(
            string continuationToken,
            int? pageSize = null, string partitionKey = null);
    }
}
