// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides Query capability
    /// </summary>
    public interface IQuery : IDisposable {

        /// <summary>
        /// Create Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IOrderedQueryable<T> CreateQuery<T>(int? pageSize = null,
            OperationOptions options = null);

        /// <summary>
        /// Run query items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        IResultFeed<IDocumentInfo<T>> RunQuery<T>(IOrderedQueryable<T> query);

        /// <summary>
        /// Continue a previously run query using continuation token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="continuationToken"></param>
        /// <param name="pageSize"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        IResultFeed<IDocumentInfo<T>> Continue<T>(string continuationToken,
            int? pageSize = null, string partitionKey = null);

        /// <summary>
        /// Drop all items that match the query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DropAsync<T>(IOrderedQueryable<T> query,
            CancellationToken ct = default);
    }
}
