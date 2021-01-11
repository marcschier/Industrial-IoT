// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage {
    using Microsoft.IIoT.Exceptions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a collection of documents in a database
    /// </summary>
    public interface IDocumentCollection {

        /// <summary>
        /// Name of the collection
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IDocumentInfo<T>> AddAsync<T>(T newItem,
            string id = null, OperationOptions options = null,
            CancellationToken ct = default);

        /// <summary>
        /// Finds an item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IDocumentInfo<T>> FindAsync<T>(string id,
            OperationOptions options = null,
            CancellationToken ct = default);

        /// <summary>
        /// Replace item
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing,
            T value, OperationOptions options = null,
            CancellationToken ct = default);

        /// <summary>
        /// Adds or updates an item.
        /// </summary>
        /// <exception cref="ResourceOutOfDateException"/>
        /// <param name="newItem"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="etag"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem,
            string id = null,
            OperationOptions options = null, string etag = null,
            CancellationToken ct = default);

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <exception cref="ResourceOutOfDateException"/>
        /// <param name="item"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync<T>(IDocumentInfo<T> item,
            OperationOptions options = null,
            CancellationToken ct = default);

        /// <summary>
        /// Delete an item by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="etag"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync<T>(string id,
            OperationOptions options = null, string etag = null,
            CancellationToken ct = default);

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
