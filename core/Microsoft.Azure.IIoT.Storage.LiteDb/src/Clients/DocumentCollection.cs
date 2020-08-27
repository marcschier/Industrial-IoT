// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Http.Exceptions;
    using Microsoft.Azure.IIoT.Http;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    using LiteDB;

    /// <summary>
    /// Wraps a cosmos db container
    /// </summary>
    internal sealed class DocumentCollection : IItemContainer, IDocuments, IQueryClient {

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        internal DocumentCollection(string name, ILiteDatabase db, 
            ContainerOptions options, ILogger logger) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IDocuments AsDocuments() {
            return this;
        }

        /// <inheritdoc/>
        public IQueryClient Query() {
            return this;
        }

        /// <inheritdoc/>
        public ISqlClient OpenSqlClient() {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IDocumentInfo<T>> FindAsync<T>(string id, CancellationToken ct,
            OperationOptions options) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            try {
                var result = Find<T>(id);
                return Task.FromResult<IDocumentInfo<T>>(result);
            }
            catch (Exception ex) {
                FilterException(ex);
                return Task.FromResult<IDocumentInfo<T>>(null);
            }
        }

        /// <inheritdoc/>
        public Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem,
            CancellationToken ct, string id, OperationOptions options, string etag) {
            try {
                var newDoc = new DocumentInfo<T>(newItem, id, _db.Mapper);
                if (!string.IsNullOrEmpty(etag)) {
                    _db.BeginTrans();
                    try {
                        // Get etag and compare with this here
                        var doc = Find<T>(id);
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
                    _db.GetCollection(Name).Upsert(newDoc.Bson);
                }
                return Task.FromResult<IDocumentInfo<T>>(newDoc);
            }
            catch (Exception ex) {
                FilterException(ex);
                return Task.FromResult<IDocumentInfo<T>>(null);
            }
        }

        /// <inheritdoc/>
        public Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing,
            T newItem, CancellationToken ct, OperationOptions options) {
            if (existing == null) {
                throw new ArgumentNullException(nameof(existing));
            }
            try {
                var newDoc = new DocumentInfo<T>(newItem, existing.Id, _db.Mapper);
                _db.BeginTrans();
                try {
                    // Get etag and compare with this here
                    var doc = Find<T>(existing.Id);
                    if (doc == null) {
                        throw new ResourceNotFoundException();
                    }
                    if (doc.Etag != existing.Etag) {
                        // Not matching etag
                        throw new ResourceOutOfDateException();
                    }
                    // Update
                    _db.GetCollection(Name).Update(newDoc.Bson);
                    _db.Commit();
                }
                catch {
                    _db.Rollback();
                    throw;
                }
                return Task.FromResult<IDocumentInfo<T>>(newDoc);
            }
            catch (Exception ex) {
                FilterException(ex);
                return Task.FromResult<IDocumentInfo<T>>(null);
            }
        }

        /// <inheritdoc/>
        public Task<IDocumentInfo<T>> AddAsync<T>(T newItem, CancellationToken ct,
            string id, OperationOptions options) {
            try {
                var doc = new DocumentInfo<T>(newItem, id, _db.Mapper);
                _db.GetCollection(Name).Insert(doc.Bson);
                return Task.FromResult<IDocumentInfo<T>>(doc);
            }
            catch (Exception ex) {
                FilterException(ex);
                return Task.FromResult<IDocumentInfo<T>>(null);
            }
        }

        /// <inheritdoc/>
        public Task DeleteAsync<T>(IDocumentInfo<T> item, CancellationToken ct,
            OperationOptions options) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            return DeleteAsync<T>(item.Id, ct, options, item.Etag);
        }

        /// <inheritdoc/>
        public Task DeleteAsync<T>(string id, CancellationToken ct,
            OperationOptions options, string etag) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            try {
                if (!string.IsNullOrEmpty(etag)) {
                    _db.BeginTrans();
                    try {
                        // Get etag and compare with this here
                        var doc = Find<T>(id);
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
                    _db.GetCollection(Name).Delete(id);
                }
            }
            catch (Exception ex) {
                FilterException(ex);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IQuery<T> CreateQuery<T>(int? pageSize, OperationOptions options) {
            return new DocumentQuery<T>(this, _db.GetCollection<T>().Query(), pageSize);
        }

        /// <inheritdoc/>
        public IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(string continuationToken,
            int? pageSize = null, string partitionKey = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        private DocumentInfo<T> Find<T>(string id) {
            var doc = _db.GetCollection(Name).FindById(id);
            var result = doc == null ? null : new DocumentInfo<T>(doc, _db.Mapper);
            return result;
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
            internal DocumentResultFeed(DocumentCollection collection,
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
                        : Enumerable.Empty<DocumentInfo<T>>();
                    if (result == null) {
                        _collection._queryStore.Remove(_continuationToken);
                    }
                    return Task.FromResult(result);
                }
            }

            private readonly DocumentCollection _collection;
            private readonly string _continuationToken;
            private readonly Queue<IEnumerable<IDocumentInfo<T>>> _items;
            private readonly object _lock = new object();
        }

        /// <summary>
        /// Query
        /// </summary>
        internal sealed class DocumentQuery<T> : IQuery<T> {

            /// <summary>
            /// Create query
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="queryable"></param>
            /// <param name="pageSize"></param>
            internal DocumentQuery(DocumentCollection collection, ILiteQueryableResult<T> queryable,
                int? pageSize) {
                _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _pageSize = pageSize;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> GetResults() {
                var results = _queryable.ToDocuments().Select(d => new DocumentInfo<T>(d));
                return new DocumentResultFeed<T>(_collection, results, _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    throw new InvalidOperationException("Already evaluated query");
                }
                return new DocumentQuery<T>(_collection, queryable.Where(predicate), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    throw new InvalidOperationException("Already evaluated query");
                }
                return new DocumentQuery<T>(_collection, queryable.OrderBy(keySelector, order), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    throw new InvalidOperationException("Already evaluated query");
                }
                return new DocumentQuery<T>(_collection, queryable.OrderByDescending(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    throw new InvalidOperationException("Already evaluated query");
                }
                return new DocumentQuery<K>(_collection, queryable.Select(selector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    throw new InvalidOperationException("Already evaluated query");
                }
                throw new NotSupportedException();
                // return new DocumentQuery<K>(_collection, queryable.Select(selector), _pageSize);
            }

            /// <inheritdoc/>
            public Task<int> CountAsync(CancellationToken ct = default) {
                return Task.FromResult(_queryable.Count());
            }

            private readonly DocumentCollection _collection;
            private readonly ILiteQueryableResult<T> _queryable;
            private readonly int? _pageSize;
        }


        /// <summary>
        /// Filter exceptions
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        internal static void FilterException(Exception ex) {
            if (ex is HttpResponseException re) {
                re.StatusCode.Validate(re.Message);
            }
            else {
                throw ex;
            }
        }

        private readonly ILiteDatabase _db;
        private readonly ContainerOptions _options;
        private readonly ILogger _logger;

        private readonly Dictionary<string, object> _queryStore =
            new Dictionary<string, object>();

    }
}
