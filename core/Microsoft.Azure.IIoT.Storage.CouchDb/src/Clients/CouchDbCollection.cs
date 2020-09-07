// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using CouchDB.Driver;
    using CouchDB.Driver.Types;

    /// <summary>
    /// Wraps a collection
    /// </summary>
    internal sealed class CouchDbCollection : IItemContainer {

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        internal CouchDbCollection(string name, ICouchDatabase<CouchDbDocument> db,
            ContainerOptions options, ILogger logger) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? new ContainerOptions();
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> FindAsync<T>(string id, CancellationToken ct,
            OperationOptions options) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            try {
                var doc = await _db.FindAsync(id);
                return doc?.ToDocumentInfo<T>();
            }
            catch (Exception ex) {
                FilterException(ex);
                return null;
            }
        }

#if FALSE

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem,
            CancellationToken ct, string id, OperationOptions options, string etag) {
            try {
                var newDoc = CouchDbDocument.CreateUpdated(newItem, id);



                if (!string.IsNullOrEmpty(etag)) {
                    _db.BeginTrans();
                    try {
                        // Get etag and compare with this here
                        var doc = FindAsync<T>(id);
                        if (doc == null) {
                            // Add new
                            _db.GetCollection(Name).Insert(newDoc.Bson);
                        }
                        else if (doc.Etag != etag) {
                            // Not matching etag
                            throw new ResourceOutOfDateException();
                        }
                        else {
                            // Update existing
                            _db.GetCollection(Name).Update(newDoc.Bson);
                        }
                        _db.Commit();
                    }
                    catch {
                        _db.Rollback();
                        throw;
                    }
                }
                else {
                    // Plain old upsert
                    newDoc = await _db.AddOrUpdateAsync(newDoc, false, ct);
                }
                return newDoc.ToDocumentInfo<T>();
            }
            catch (Exception ex) {
                FilterException(ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing,
            T newItem, CancellationToken ct, OperationOptions options) {
            if (existing == null) {
                throw new ArgumentNullException(nameof(existing));
            }
            if (string.IsNullOrEmpty(existing.Id)) {
                throw new ArgumentNullException(nameof(existing.Id));
            }
            try {
                var newDoc = CouchDbDocument.CreateUpdated(newItem, existing.Id);
                _db.BeginTrans();
                try {
                    // Get etag and compare with this here
                    var doc = await FindAsync<T>(existing.Id);
                    if (doc == null) {
                        throw new ResourceNotFoundException();
                    }
                    if (doc.Etag != existing.Etag) {
                        // Not matching etag
                        throw new ResourceOutOfDateException();
                    }
                    // Update
                    newDoc = await _db.AddOrUpdateAsync(newDoc, false, ct);
                    _db.Commit();
                }
                catch {
                    _db.Rollback();
                    throw;
                }
                return newDoc.ToDocumentInfo<T>();
            }
            catch (Exception ex) {
                FilterException(ex);
                return null;
            }
        }
#endif

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> AddAsync<T>(T newItem, CancellationToken ct,
            string id, OperationOptions options) {
            try {
                var newDoc = CouchDbDocument.CreateUpdated(newItem, id);
                newDoc = await _db.AddAsync(newDoc, false, ct);
                return newDoc.ToDocumentInfo<T>();
            }
            catch (Exception ex) {
                FilterException(ex);
                return null;
            }
        }

#if FALSE

        /// <inheritdoc/>
        public Task DeleteAsync<T>(IDocumentInfo<T> item, CancellationToken ct,
            OperationOptions options) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            if (string.IsNullOrEmpty(item.Id)) {
                throw new ArgumentNullException(nameof(item.Id));
            }
            return DeleteAsync<T>(item.Id, ct, options, item.Etag);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(string id, CancellationToken ct,
            OperationOptions options, string etag) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            try {
                if (!string.IsNullOrEmpty(etag)) {
                    _db.BeginTrans();
                    try {
                        // Get etag and compare with this here
                        var doc = await FindAsync<T>(id);
                        if (doc == null) {
                            throw new ResourceNotFoundException();
                        }
                        if (doc.Etag != etag) {
                            throw new ResourceOutOfDateException();
                        }
                        _db.GetCollection(Name).Delete(id);
                        _db.Commit();
                    }
                    catch {
                        _db.Rollback();
                        throw;
                    }
                }
                else {
                    var doc = await _db.FindAsync(id);
                    await _db.RemoveAsync(doc, false, ct);
                }
            }
            catch (Exception ex) {
                FilterException(ex);
            }
        }

        /// <inheritdoc/>
        public IQuery<T> CreateQuery<T>(int? pageSize, OperationOptions options) {
            return new ServerSideQuery<T>(this, _db, pageSize);
        }

        /// <inheritdoc/>
        public IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(string continuationToken,
            int? pageSize, string partitionKey) {
            if (_queryStore.TryGetValue(continuationToken, out var feed)) {
                var result = feed as IResultFeed<IDocumentInfo<T>>;
                if (result == null) {
                    _logger.Error("Continuation {continuation} type mismatch.",
                        continuationToken);
                }
                return result;
            }
            _logger.Error("Continuation {continuation} not found",
                continuationToken);
            return null;
        }

        /// <summary>
        /// Find item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<IDocumentInfo<T>> FindAsync<T>(string id) {
            var doc = await _db.FindAsync(id);
            return doc?.ToDocumentInfo<T>();
        }




        /// <summary>
        /// Result feed
        /// </summary>
        internal sealed class DocumentResultFeed<T> : IResultFeed<IDocumentInfo<T>> {

            /// <inheritdoc/>
            public string ContinuationToken {
                get {
                    lock (_lock) {
                        if (_items.Count == 0) {
                            return null;
                        }
                        return _continuationToken;
                    }
                }
            }

            /// <summary>
            /// Create feed
            /// </summary>
            internal DocumentResultFeed(CouchDbCollection collection,
                IEnumerable<IDocumentInfo<T>> documents, int? pageSize) {
                var feed = (pageSize == null) ?
                    documents.YieldReturn() : documents.Batch(pageSize.Value);
                _items = new Queue<IEnumerable<IDocumentInfo<T>>>(feed);
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _continuationToken = Guid.NewGuid().ToString();
                _collection._queryStore.Add(_continuationToken, this);
            }

            /// <inheritdoc/>
            public bool HasMore() {
                lock (_lock) {
                    if (_items.Count == 0) {
                        _collection._queryStore.Remove(_continuationToken);
                        return false;
                    }
                    return true;
                }
            }

            /// <inheritdoc/>
            public Task<IEnumerable<IDocumentInfo<T>>> ReadAsync(CancellationToken ct) {
                lock (_lock) {
                    var result = _items.Count != 0 ? _items.Dequeue()
                        : Enumerable.Empty<CouchDbDocument>();
                    if (result == null) {
                        _collection._queryStore.Remove(_continuationToken);
                    }
                    return Task.FromResult(result);
                }
            }

            private readonly CouchDbCollection _collection;
            private readonly string _continuationToken;
            private readonly Queue<IEnumerable<IDocumentInfo<T>>> _items;
            private readonly object _lock = new object();
        }

        /// <summary>
        /// Server side
        /// </summary>
        internal sealed class ServerSideQuery<T> : IQuery<T> {

            /// <summary>
            /// Create query
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="queryable"></param>
            /// <param name="pageSize"></param>
            internal ServerSideQuery(CouchDbCollection collection,
                IQueryable<T> queryable, int? pageSize) {
                _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _pageSize = pageSize;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> GetResults() {

                var results = _queryable.To ToDocuments()
                    .Select(d => new CouchDbDocument<T>(d, _collection._db.Mapper));
                return new DocumentResultFeed<T>(_collection, results, _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
                return new ServerSideQuery<T>(_collection,
                    _queryable.Where(predicate), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector) {
                return new ServerSideQuery<T>(_collection,
                    _queryable.OrderBy(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
                return new ServerSideQuery<T>(_collection,
                    _queryable.OrderByDescending(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
                return new ServerSideQuery<K>(_collection,
                    _queryable.Select(selector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Take(int maxRecords) {
                return new ServerSideQuery<T>(_collection,
                    _queryable.Take(maxRecords), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Distinct() {
                return new ServerSideQuery<T>(_collection,
                    _queryable.Distinct(), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
                return new ServerSideQuery<K>(_collection,
                    _queryable.SelectMany(selector), _pageSize);
            }

            /// <inheritdoc/>
            public Task<int> CountAsync(CancellationToken ct = default) {
                return Task.FromResult(_queryable.Count());
            }

            private readonly CouchDbCollection _collection;
            private readonly IQueryable<T> _queryable;
            private readonly int? _pageSize;
        }
#endif

        /// <summary>
        /// Filter exceptions
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        internal static void FilterException(Exception ex) {
            throw ex;
        }









        public Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing, T value, CancellationToken ct = default, OperationOptions options = null) {
            throw new NotImplementedException();
        }

        public Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem, CancellationToken ct = default, string id = null, OperationOptions options = null, string etag = null) {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(IDocumentInfo<T> item, CancellationToken ct = default, OperationOptions options = null) {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(string id, CancellationToken ct = default, OperationOptions options = null, string etag = null) {
            throw new NotImplementedException();
        }

        public IQuery<T> CreateQuery<T>(int? pageSize = null, OperationOptions options = null) {
            throw new NotImplementedException();
        }

        public IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(string continuationToken, int? pageSize = null, string partitionKey = null) {
            throw new NotImplementedException();
        }






        private readonly ICouchDatabase<CouchDbDocument> _db;
        private readonly ContainerOptions _options;
        private readonly ILogger _logger;

        private readonly Dictionary<string, object> _queryStore =
            new Dictionary<string, object>();
    }
}
