// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    using LiteDB;

    /// <summary>
    /// Wraps a collection
    /// </summary>
    internal sealed class DocumentCollection : IItemContainer {

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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? new ContainerOptions();
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
                var newDoc = new DocumentInfo<T>(newItem, _db.Mapper, id);
                if (!string.IsNullOrEmpty(etag) && !string.IsNullOrEmpty(newDoc.Id)) {
                    _db.BeginTrans();
                    try {
                        // Get etag and compare with this here
                        var doc = Find<T>(newDoc.Id);
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
                _db.Checkpoint();
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
            if (string.IsNullOrEmpty(existing.Id)) {
                throw new ArgumentNullException(nameof(existing.Id));
            }
            if (newItem == null) {
                throw new ArgumentNullException(nameof(newItem));
            }
            try {
                var newDoc = new DocumentInfo<T>(newItem, _db.Mapper, existing.Id);
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
                _db.Checkpoint();
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
            if (newItem == null) {
                throw new ArgumentNullException(nameof(newItem));
            }
            try {
                var newDoc = new DocumentInfo<T>(newItem, _db.Mapper, id);
                _db.GetCollection(Name).Insert(newDoc.Bson);
                _db.Checkpoint();
                return Task.FromResult<IDocumentInfo<T>>(newDoc);
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
            if (string.IsNullOrEmpty(item.Id)) {
                throw new ArgumentNullException(nameof(item.Id));
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
                    _db.GetCollection(Name).Delete(id);
                }
                _db.Checkpoint();
            }
            catch (Exception ex) {
                FilterException(ex);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IQuery<T> CreateQuery<T>(int? pageSize, OperationOptions options) {
            return new ServerSideQuery<T>(this, _db.GetCollection<T>(Name).Query(), pageSize);
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
        /// Client side
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal sealed class ClientSideQuery<T> : IQuery<T> {

            /// <summary>
            /// Create query
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="queryable"></param>
            /// <param name="pageSize"></param>
            internal ClientSideQuery(DocumentCollection collection, IQueryable<T> queryable,
                int? pageSize) {
                _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _pageSize = pageSize;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> GetResults() {
                var results = _queryable.AsEnumerable()
                    .Select(d => new DocumentInfo<T>(d, _collection._db.Mapper));
                return new DocumentResultFeed<T>(_collection, results, _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
                return new ClientSideQuery<T>(_collection,
                    _queryable.Where(predicate), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order) {
                return new ClientSideQuery<T>(_collection,
                    _queryable.OrderBy(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
                return new ClientSideQuery<T>(_collection,
                    _queryable.OrderByDescending(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
                return new ClientSideQuery<K>(_collection,
                    _queryable.Select(selector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
                return new ClientSideQuery<K>(_collection,
                    _queryable.SelectMany(selector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Take(int maxRecords) {
                return new ClientSideQuery<T>(_collection,
                    _queryable.Take(maxRecords), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Distinct() {
                return new ClientSideQuery<T>(_collection,
                    _queryable.Distinct(), _pageSize);
            }

            /// <inheritdoc/>
            public Task<int> CountAsync(CancellationToken ct = default) {
                return Task.FromResult(_queryable.Count());
            }

            private readonly IQueryable<T> _queryable;
            private readonly DocumentCollection _collection;
            private readonly int? _pageSize;
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
            internal ServerSideQuery(DocumentCollection collection,
                ILiteQueryableResult<T> queryable, int? pageSize) {
                _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _pageSize = pageSize;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> GetResults() {

                var results = _queryable.ToDocuments()
                    .Select(d => new DocumentInfo<T>(d, _collection._db.Mapper));
                return new DocumentResultFeed<T>(_collection, results, _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    return Execute().Where(predicate);
                }
                return new ServerSideQuery<T>(_collection,
                    queryable.Where(predicate), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    return Execute().OrderBy(keySelector, order);
                }
                return new ServerSideQuery<T>(_collection,
                    queryable.OrderBy(keySelector, order), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
                if (!(_queryable is ILiteQueryable<T> queryable)) {
                    return Execute().OrderByDescending(keySelector);
                }
                return new ServerSideQuery<T>(_collection,
                    queryable.OrderByDescending(keySelector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
                if (!(_queryable is ILiteQueryable<T> queryable) || typeof(T) != typeof(K)) {
                    return Execute().Select(selector);
                }
                return new ServerSideQuery<K>(_collection,
                    queryable.Select(selector), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Take(int maxRecords) {
                return new ServerSideQuery<T>(_collection,
                    _queryable.Limit(maxRecords), _pageSize);
            }

            /// <inheritdoc/>
            public IQuery<T> Distinct() {
                return Execute().Distinct();
            }

            /// <inheritdoc/>
            public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
                return Execute().SelectMany(selector);
            }

            /// <inheritdoc/>
            public Task<int> CountAsync(CancellationToken ct = default) {
                return Task.FromResult(_queryable.Count());
            }

            /// <summary>
            /// Execute query and return client side query as continuation
            /// </summary>
            /// <returns></returns>
            public IQuery<T> Execute() {
                return new ClientSideQuery<T>(_collection,
                    _queryable.ToEnumerable().AsQueryable(), _pageSize);
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
            if (ex is LiteException lex) {
                switch (lex.ErrorCode) {
                    case LiteException.FILE_NOT_FOUND:
                        throw new ResourceNotFoundException(
                            nameof(LiteException.FILE_NOT_FOUND), lex);
                    case LiteException.DATABASE_SHUTDOWN:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.DATABASE_SHUTDOWN), lex);
                    case LiteException.INVALID_DATABASE:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.INVALID_DATABASE), lex);
                    case LiteException.FILE_SIZE_EXCEEDED:
                        throw new ResourceExhaustionException(
                            nameof(LiteException.FILE_SIZE_EXCEEDED), lex);
                    case LiteException.COLLECTION_LIMIT_EXCEEDED:
                        throw new ResourceExhaustionException(
                            nameof(LiteException.COLLECTION_LIMIT_EXCEEDED), lex);
                    case LiteException.INDEX_DROP_ID:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.INDEX_DROP_ID), lex);
                    case LiteException.INDEX_DUPLICATE_KEY:
                        throw new ResourceConflictException(
                            nameof(LiteException.INDEX_DUPLICATE_KEY), lex);
                    case LiteException.INVALID_INDEX_KEY:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.INVALID_INDEX_KEY), lex);
                    case LiteException.INDEX_NOT_FOUND:
                        throw new ResourceNotFoundException(
                            nameof(LiteException.INDEX_NOT_FOUND), lex);
                    case LiteException.LOCK_TIMEOUT:
                        throw new TimeoutException(
                            nameof(LiteException.LOCK_TIMEOUT), lex);
                    case LiteException.INVALID_COMMAND:
                        throw new NotSupportedException(
                            nameof(LiteException.INVALID_COMMAND), lex);
                    case LiteException.ALREADY_EXISTS_COLLECTION_NAME:
                        throw new ResourceConflictException(
                            nameof(LiteException.ALREADY_EXISTS_COLLECTION_NAME), lex);
                    case LiteException.ALREADY_OPEN_DATAFILE:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.ALREADY_OPEN_DATAFILE), lex);
                    case LiteException.INVALID_TRANSACTION_STATE:
                        throw new ResourceInvalidStateException(
                            nameof(LiteException.INVALID_TRANSACTION_STATE), lex);
                    case LiteException.INDEX_NAME_LIMIT_EXCEEDED:
                        throw new ArgumentException(
                            nameof(LiteException.INDEX_NAME_LIMIT_EXCEEDED), lex);
                    case LiteException.INVALID_INDEX_NAME:
                        throw new ArgumentException(
                            nameof(LiteException.INVALID_INDEX_NAME), lex);
                    case LiteException.INVALID_COLLECTION_NAME:
                        throw new ArgumentException(
                            nameof(LiteException.INVALID_COLLECTION_NAME), lex);
                    case LiteException.COLLECTION_NOT_FOUND:
                        throw new ResourceNotFoundException(
                            nameof(LiteException.COLLECTION_NOT_FOUND), lex);
                    case LiteException.COLLECTION_ALREADY_EXIST:
                        throw new ResourceConflictException(
                            nameof(LiteException.COLLECTION_ALREADY_EXIST), lex);
                    case LiteException.INDEX_ALREADY_EXIST:
                        throw new ResourceConflictException(
                            nameof(LiteException.INDEX_ALREADY_EXIST), lex);
                    case LiteException.INVALID_FORMAT:
                        throw new FormatException(
                            nameof(LiteException.INVALID_FORMAT), lex);
                    case LiteException.DOCUMENT_MAX_DEPTH:
                        throw new SerializerException(
                            nameof(LiteException.DOCUMENT_MAX_DEPTH), lex);
                    case LiteException.UNEXPECTED_TOKEN:
                        throw new SerializerException(
                            nameof(LiteException.UNEXPECTED_TOKEN), lex);
                    case LiteException.INVALID_DATA_TYPE:
                        throw new SerializerException(
                            nameof(LiteException.INVALID_DATA_TYPE), lex);
                    case LiteException.PROPERTY_NOT_MAPPED:
                        throw new SerializerException(
                            nameof(LiteException.PROPERTY_NOT_MAPPED), lex);
                    case LiteException.INVALID_TYPED_NAME:
                        throw new SerializerException(
                            nameof(LiteException.INVALID_TYPED_NAME), lex);
                    case LiteException.PROPERTY_READ_WRITE:
                        throw new SerializerException(
                            nameof(LiteException.PROPERTY_READ_WRITE), lex);
                    case LiteException.INVALID_INITIALSIZE:
                        throw new ArgumentException(
                            nameof(LiteException.INVALID_INITIALSIZE), lex);
                    case LiteException.INVALID_NULL_CHAR_STRING:
                        throw new SerializerException(
                            nameof(LiteException.INVALID_NULL_CHAR_STRING), lex);
                }
            }
            throw ex;
        }

        private readonly ILiteDatabase _db;
        private readonly ContainerOptions _options;
        private readonly ILogger _logger;

        private readonly Dictionary<string, object> _queryStore =
            new Dictionary<string, object>();
    }
}
