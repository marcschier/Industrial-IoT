// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Default {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// In memory database service (for testing)
    /// </summary>
    public sealed class MemoryDatabase : IDatabaseServer {

        /// <summary>
        /// Create database
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializer"></param>
        /// <param name="queryEngine"></param>
        public MemoryDatabase(ILogger logger, IJsonSerializer serializer,
            IQueryEngine queryEngine = null) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryEngine = queryEngine;
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string id, DatabaseOptions options) {
            return Task.FromResult<IDatabase>(_databases.GetOrAdd(id ?? "",
                k => new ItemContainerDatabase(this)));
        }

        /// <summary>
        /// In memory database
        /// </summary>
        private class ItemContainerDatabase : IDatabase {

            /// <summary>
            /// Create database
            /// </summary>
            /// <param name="queryEngine"></param>
            public ItemContainerDatabase(MemoryDatabase queryEngine) {
                _outer = queryEngine;
            }

            /// <inheritdoc/>
            public Task DeleteContainerAsync(string id) {
                _containers.TryRemove(id, out var tmp);
                tmp?.Dispose();
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public Task<IEnumerable<string>> ListContainersAsync(CancellationToken ct) {
                return Task.FromResult<IEnumerable<string>>(_containers.Keys);
            }

            /// <inheritdoc/>
            public Task<IItemContainer> OpenContainerAsync(string id,
                ContainerOptions options) {
                return Task.FromResult<IItemContainer>(_containers.GetOrAdd(id ?? "",
                    k => new ItemContainer(id, _outer)));
            }

            private readonly ConcurrentDictionary<string, ItemContainer> _containers =
                new ConcurrentDictionary<string, ItemContainer>();
            private readonly MemoryDatabase _outer;
        }

        /// <summary>
        /// In memory container
        /// </summary>
        private class ItemContainer : IItemContainer, IDocuments,
            ISqlClient, IQuery {

            /// <inheritdoc/>
            public string Name { get; }

            /// <summary>
            /// Create service
            /// </summary>
            /// <param name="name"></param>
            /// <param name="outer"></param>
            public ItemContainer(string name, MemoryDatabase outer) {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                _outer = outer;
            }

            /// <inheritdoc/>
            public Task<IDocumentInfo<T>> AddAsync<T>(T newItem,
                CancellationToken ct, string id, OperationOptions options) {
                var item = _outer._serializer.FromObject(newItem);
                var newDoc = new Document<T>(id, item, options?.PartitionKey);
                lock (_data) {
                    if (_data.TryGetValue(newDoc.Id, out var existing)) {
                        return Task.FromException<IDocumentInfo<T>>(
                            new ConflictingResourceException(newDoc.Id));
                    }
                    AddDocument(newDoc);
                    return Task.FromResult<IDocumentInfo<T>>(newDoc);
                }
            }

            /// <inheritdoc/>
            public Task DeleteAsync<T>(IDocumentInfo<T> item, CancellationToken ct,
                OperationOptions options) {
                if (item == null) {
                    throw new ArgumentNullException(nameof(item));
                }
                return DeleteAsync<T>(item.Id, ct, new OperationOptions {
                    PartitionKey = item.PartitionKey
                }, item.Etag);
            }

            /// <inheritdoc/>
            public Task DeleteAsync<T>(string id, CancellationToken ct,
                OperationOptions options, string etag) {
                if (string.IsNullOrEmpty(id)) {
                    throw new ArgumentNullException(nameof(id));
                }
                lock (_data) {
                    if (!_data.TryGetValue(id, out var doc)) {
                        return Task.FromException(
                            new ResourceNotFoundException(id));
                    }
                    if (!string.IsNullOrEmpty(etag) && etag != doc.Etag) {
                        return Task.FromException(
                            new ResourceOutOfDateException(etag));
                    }
                    _data.Remove(id);
                    return Task.CompletedTask;
                }
            }

            /// <inheritdoc/>
            public Task<IDocumentInfo<T>> FindAsync<T>(string id, CancellationToken ct,
                OperationOptions options) {
                if (string.IsNullOrEmpty(id)) {
                    throw new ArgumentNullException(nameof(id));
                }
                lock (_data) {
                    _data.TryGetValue(id, out var item);
                    return Task.FromResult(item as IDocumentInfo<T>);
                }
            }

            /// <inheritdoc/>
            public Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing, T value,
                CancellationToken ct, OperationOptions options) {
                if (existing == null) {
                    throw new ArgumentNullException(nameof(existing));
                }
                var item = _outer._serializer.FromObject(value);
                var newDoc = new Document<T>(existing.Id, item, existing.PartitionKey);
                lock (_data) {
                    if (_data.TryGetValue(newDoc.Id, out var doc)) {
                        if (!string.IsNullOrEmpty(existing.Etag) && doc.Etag != existing.Etag) {
                            return Task.FromException<IDocumentInfo<T>>(
                                new ResourceOutOfDateException(existing.Etag));
                        }
                        _data.Remove(newDoc.Id);
                    }
                    else {
                        return Task.FromException<IDocumentInfo<T>>(
                            new ResourceNotFoundException(newDoc.Id));
                    }
                    AddDocument(newDoc);
                    return Task.FromResult<IDocumentInfo<T>>(newDoc);
                }
            }

            /// <inheritdoc/>
            public Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem, CancellationToken ct,
                string id, OperationOptions options, string etag) {
                var item = _outer._serializer.FromObject(newItem);
                var newDoc = new Document<T>(id, item, options?.PartitionKey);
                lock (_data) {
                    if (_data.TryGetValue(newDoc.Id, out var doc)) {
                        if (!string.IsNullOrEmpty(etag) && doc.Etag != etag) {
                            return Task.FromException<IDocumentInfo<T>>(
                                new ResourceOutOfDateException(etag));
                        }
                        _data.Remove(newDoc.Id);
                    }

                    AddDocument(newDoc);
                    return Task.FromResult<IDocumentInfo<T>>(newDoc);
                }
            }

            /// <summary>
            /// Checks size of newly added document
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="newDoc"></param>
            private void AddDocument<T>(Document<T> newDoc) {
                const int kMaxDocSize = 2 * 1024 * 1024;  // 2 meg like in cosmos
                var bytes = _outer._serializer.SerializeToBytes(newDoc.Value);
                if (bytes.Length > kMaxDocSize) {
                    throw new ResourceTooLargeException(newDoc.ToString(),
                        bytes.Length, kMaxDocSize);
                }
                _data.Add(newDoc.Id, newDoc);
#if LOG_VERBOSE
                _logger.Information("{@doc}", newDoc);
#endif
            }

            /// <inheritdoc/>
            public IDocuments AsDocuments() {
                return this;
            }

            /// <inheritdoc/>
            public ISqlClient OpenSqlClient() {
                return this;
            }

            /// <inheritdoc/>
            public IQuery Query() {
                return this;
            }

            /// <inheritdoc/>
            public IOrderedQueryable<T> CreateQuery<T>(int? pageSize, OperationOptions options) {
                return new MemoryDocumentQuery<T>(_data.Values.AsQueryable(), pageSize);
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> GetResults<T>(IQueryable<T> query) {
                var documentQuery = query as MemoryDocumentQuery<T>;
                var documents = documentQuery.Documents();
                var results = documents
                    .Select(d => new Document<T>(d.Id, d.Value, d.PartitionKey));
                var feed = (documentQuery.PageSize == null) ?
                    results.YieldReturn() : results.Batch(documentQuery.PageSize.Value);
                return new MemoryFeed<IDocumentInfo<T>>(this,
                    new Queue<IEnumerable<IDocumentInfo<T>>>(feed));
            }

            /// <inheritdoc/>
            public Task DropAsync<T>(IQueryable<T> query, CancellationToken ct) {
                var documentQuery = query as MemoryDocumentQuery<T>;
                var documents = documentQuery.Documents();
                foreach (var item in documents) {
                    _data.Remove(item.Id);
                }
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(string continuationToken,
                int? pageSize, string partitionKey) {
                if (_queryStore.TryGetValue(continuationToken, out var feed)) {
                    var result = feed as IResultFeed<IDocumentInfo<T>>;
                    if (result == null) {
                        _outer._logger.Error("Continuation {continuation} type mismatch.",
                            continuationToken);
                    }
                    return result;
                }
                _outer._logger.Error("Continuation {continuation} not found",
                    continuationToken);
                return null;
            }

            /// <inheritdoc/>
            public IResultFeed<IDocumentInfo<T>> Query<T>(string queryString,
                IDictionary<string, object> parameters, int? pageSize,
                string partitionKey) {
                queryString = FormatQueryString(queryString, parameters);
                var documents = _outer._queryEngine?.ExecuteSql(_data.Values, queryString);
                if (documents == null) {
                    throw new NotSupportedException("Query not supported");
                }
                var results = documents
                    .Select(d => new Document<T>(d.Id, d.Value, d.PartitionKey));
                var feed = (pageSize == null) ?
                    results.YieldReturn() : results.Batch(pageSize.Value);
                return new MemoryFeed<IDocumentInfo<T>>(this,
                    new Queue<IEnumerable<IDocumentInfo<T>>>(feed));
            }

            /// <inheritdoc/>
            public Task DropAsync<T>(string queryString,
                IDictionary<string, object> parameters, string partitionKey,
                CancellationToken ct) {
                queryString = FormatQueryString(queryString, parameters);
                var documents = _outer._queryEngine?.ExecuteSql(_data.Values, queryString);
                if (documents == null) {
                    throw new NotSupportedException("Query not supported");
                }
                foreach (var item in documents) {
                    _data.Remove(item.Id);
                }
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            public void Dispose() {
            }

            /// <summary>
            /// Wraps a document value
            /// </summary>
            internal abstract class MemoryDocument : IDocumentInfo<VariantValue> {

                /// <summary>
                /// Create memory document
                /// </summary>
                /// <param name="value"></param>
                /// <param name="id"></param>
                /// <param name="partitionKey"></param>
                protected MemoryDocument(VariantValue value, string id, string partitionKey) {
                    Value = value;
                    Etag = Value.GetValueOrDefault("_etag", string.Empty);
                    if (string.IsNullOrEmpty(Etag)) {
                        Etag = Guid.NewGuid().ToString();
                        Value["_etag"].AssignValue(Etag);
                    }
                    Id = id ?? Value.GetValueOrDefault("id", string.Empty);
                    if (string.IsNullOrEmpty(Id)) {
                        Id = Guid.NewGuid().ToString();
                        Value["id"].AssignValue(Id);
                    }
                    PartitionKey = partitionKey ?? Value.GetValueOrDefault("__pk", string.Empty);
                    if (string.IsNullOrEmpty(PartitionKey)) {
                        PartitionKey = Guid.NewGuid().ToString();
                        Value["__pk"].AssignValue(PartitionKey);
                    }
                }

                /// <inheritdoc/>
                public string Id { get; }

                /// <inheritdoc/>
                public string PartitionKey { get; }

                /// <inheritdoc/>
                public string Etag { get; }

                /// <inheritdoc/>
                public VariantValue Value { get; }

                /// <summary>
                /// Get typed value
                /// </summary>
                /// <param name="type"></param>
                /// <returns></returns>
                public object Get(Type type) {
                    return Value.ConvertTo(type);
                }

                /// <inheritdoc/>
                public override bool Equals(object obj) {
                    if (obj is MemoryDocument wrapper) {
                        return VariantValue.DeepEquals(Value, wrapper.Value);
                    }
                    return false;
                }

                /// <inheritdoc/>
                public override int GetHashCode() {
                    return EqualityComparer<VariantValue>.Default.GetHashCode(Value);
                }

                /// <inheritdoc/>
                public override string ToString() {
                    return Value.ToString();
                }

                /// <inheritdoc/>
                public static bool operator ==(MemoryDocument o1, MemoryDocument o2) =>
                    o1.Equals(o2);

                /// <inheritdoc/>
                public static bool operator !=(MemoryDocument o1, MemoryDocument o2) =>
                    !(o1 == o2);
            }

            public class MemoryDocumentQuery<T> : ExpressionVisitor, IOrderedQueryable<T>, IQueryProvider {

                /// <inheritdoc/>
                public Type ElementType => typeof(T);

                /// <inheritdoc/>
                public Expression Expression { get; }

                /// <inheritdoc/>
                public IQueryProvider Provider => this;

                /// <summary>
                /// Page size to paginate the query
                /// </summary>
                public int? PageSize { get; }

                private MemoryDocumentQuery(IQueryable source, Expression e, int? pageSize) {
                    Expression = e ?? throw new ArgumentNullException("e");
                    _source = source;
                    PageSize = pageSize;
                }

                public MemoryDocumentQuery(IQueryable source, int? pageSize) {
                    Expression = Expression.Constant(this);
                    _source = source;
                    PageSize = pageSize;
                }

                /// <inheritdoc/>
                public IEnumerator<T> GetEnumerator() {
                    return ((IEnumerable<T>)ExecuteEnumerable()).GetEnumerator();
                }

                /// <inheritdoc/>
                IEnumerator IEnumerable.GetEnumerator() {
                    return ExecuteEnumerable().GetEnumerator();
                }

                /// <inheritdoc/>
                public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
                    return new MemoryDocumentQuery<TElement>(_source, expression, PageSize);
                }

                /// <inheritdoc/>
                public IQueryable CreateQuery(Expression expression) {
                    if (expression == null) {
                        throw new ArgumentNullException("expression");
                    }
                    var elementType = expression.Type.GetGenericArguments().First();
                    var result = (IQueryable)Activator.CreateInstance(
                        typeof(MemoryDocumentQuery<>).MakeGenericType(elementType),
                        new object[] { _source, expression, PageSize });
                    return result;
                }

                /// <inheritdoc/>
                public TResult Execute<TResult>(Expression expression) {
                    if (expression == null) {
                        throw new ArgumentNullException("expression");
                    }

                    var result = (this as IQueryProvider).Execute(expression);
                    return (TResult)result;
                }

                /// <inheritdoc/>
                public object Execute(Expression expression) {
                    if (expression == null) {
                        throw new ArgumentNullException("expression");
                    }

                    var translated = Visit(expression);
                    return _source.Provider.Execute(translated);
                }

                internal IEnumerable ExecuteEnumerable() {
                    var translated = Visit(Expression);
                    return _source.Provider.CreateQuery(translated);
                }

                internal IEnumerable<MemoryDocument> Documents() {
                    var translated = Visit(Expression);
                    return (IEnumerable<MemoryDocument>)_source.Provider.CreateQuery(translated);
                }


                /// <inheritdoc/>
                protected override Expression VisitConstant(ConstantExpression c) {
                    return c.Type == typeof(MemoryDocumentQuery<T>) ?
                        _source.Expression : base.VisitConstant(c);
                }

                /// <inheritdoc/>
                protected override Expression VisitLambda<S>(Expression<S> node) {
                    return Expression.Lambda(Visit(node.Body),
                        node.Parameters.Select(p => Expression.Parameter(typeof(MemoryDocument), p.Name)));
                }

                /// <inheritdoc/>
                protected override Expression VisitBinary(BinaryExpression node) {
                    return base.VisitBinary(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitDynamic(DynamicExpression node) {
                    return base.VisitDynamic(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitExtension(Expression node) {
                    return base.VisitExtension(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitIndex(IndexExpression node) {
                    return base.VisitIndex(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitInvocation(InvocationExpression node) {
                    return base.VisitInvocation(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitMember(MemberExpression node) {
                    return base.VisitMember(node);
                }

                /// <inheritdoc/>
                protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
                    return base.VisitMemberAssignment(node);
                }

                /// <inheritdoc/>
                protected override MemberBinding VisitMemberBinding(MemberBinding node) {
                    return base.VisitMemberBinding(node);
                }

                /// <inheritdoc/>
                protected override Expression VisitMemberInit(MemberInitExpression node) {
                    return base.VisitMemberInit(node);
                }

                /// <inheritdoc/>
                protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
                    return base.VisitMemberListBinding(node);
                }

                protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
                    return base.VisitMemberMemberBinding(node);
                }

                protected override Expression VisitMethodCall(MethodCallExpression node) {
                    return base.VisitMethodCall(node);
                }

                protected override Expression VisitNew(NewExpression node) {
                    return base.VisitNew(node);
                }

                protected override Expression VisitNewArray(NewArrayExpression node) {
                    return base.VisitNewArray(node);
                }

                protected override Expression VisitParameter(ParameterExpression node) {
                    return Expression.Parameter(typeof(MemoryDocument), node.Name);
                }

                protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
                    return base.VisitRuntimeVariables(node);
                }

                protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
                    return base.VisitTypeBinary(node);
                }

                protected override Expression VisitUnary(UnaryExpression node) {
                    return base.VisitUnary(node);
                }

                private readonly IQueryable _source;
            }


            /// <summary>
            /// Wraps a document value
            /// </summary>
            private class Document<T> : MemoryDocument, IDocumentInfo<T> {

                /// <summary>
                /// Create memory document
                /// </summary>
                /// <param name="id"></param>
                /// <param name="value"></param>
                /// <param name="partitionKey"></param>
                public Document(string id, VariantValue value, string partitionKey) :
                    base(value, id, partitionKey) {
                }

                /// <inheritdoc/>
                T IDocumentInfo<T>.Value => Value.ConvertTo<T>();
            }

            /// <summary>
            /// Memory feed
            /// </summary>
            private class MemoryFeed<T> : IResultFeed<T> {

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
                /// <param name="container"></param>
                /// <param name="items"></param>
                public MemoryFeed(ItemContainer container, Queue<IEnumerable<T>> items) {
                    _container = container;
                    _items = items;
                    _continuationToken = Guid.NewGuid().ToString();
                    _container._queryStore.Add(_continuationToken, this);
                }

                /// <inheritdoc/>
                public void Dispose() { }

                /// <inheritdoc/>
                public bool HasMore() {
                    lock (_lock) {
                        if (_items.Count == 0) {
                            _container._queryStore.Remove(_continuationToken);
                            return false;
                        }
                        return true;
                    }
                }

                /// <inheritdoc/>
                public Task<IEnumerable<T>> ReadAsync(CancellationToken ct) {
                    lock (_lock) {
                        var result = _items.Count != 0 ? _items.Dequeue() : Enumerable.Empty<T>();
                        if (result == null) {
                            _container._queryStore.Remove(_continuationToken);
                        }
                        return Task.FromResult(result);
                    }
                }

                private readonly ItemContainer _container;
                private readonly string _continuationToken;
                private readonly Queue<IEnumerable<T>> _items;
                private readonly object _lock = new object();
            }


            /// <summary>
            /// Format query string
            /// </summary>
            /// <param name="query"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            private string FormatQueryString(string query,
                IDictionary<string, object> parameters) {
                if (parameters != null) {
                    foreach (var parameter in parameters) {
                        var value = parameter.Value.ToString();
                        if (value is string) {
                            value = "'" + value + "'";
                        }
                        query = query.Replace(parameter.Key + " ", value + " ");
                    }
                }
                return query;
            }

            private readonly MemoryDatabase _outer;
            private readonly Dictionary<string, object> _queryStore =
                new Dictionary<string, object>();
            private readonly Dictionary<string, MemoryDocument> _data =
                new Dictionary<string, MemoryDocument>();
        }

        private readonly ConcurrentDictionary<string, ItemContainerDatabase> _databases =
            new ConcurrentDictionary<string, ItemContainerDatabase>();
        private readonly IJsonSerializer _serializer;
        private readonly IQueryEngine _queryEngine;
        private readonly ILogger _logger;
    }
}
