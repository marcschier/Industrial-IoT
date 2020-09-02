// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Cosmos.Linq;
    using Serilog;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Queryable wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DocumentQuery<T> : IQuery<T> {

        /// <summary>
        /// Create query from queryable
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        internal DocumentQuery(IQueryable<T> queryable, ISerializer serializer,
            ILogger logger) {
            _queryable = queryable;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc/>
        public IResultFeed<IDocumentInfo<T>> GetResults() {
            return new DocumentInfoFeed<T>(_queryable.ToStreamIterator(),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(CancellationToken ct) {
            return await _queryable.CountAsync();
        }

        /// <inheritdoc/>
        public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1) {
            return new DocumentQuery<T>(_queryable.OrderBy(keySelector),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
            return new DocumentQuery<T>(_queryable.OrderByDescending(keySelector),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
            return new DocumentQuery<K>(_queryable.Select(selector),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
            return new DocumentQuery<T>(_queryable.Where(predicate),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
            return new DocumentQuery<K>(_queryable.SelectMany(selector),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<T> Take(int maxRecords) {
            return new DocumentQuery<T>(_queryable.Take(maxRecords),
                _serializer, _logger);
        }

        /// <inheritdoc/>
        public IQuery<T> Distinct() {
            return new DocumentQuery<T>(_queryable.Distinct(),
                _serializer, _logger);
        }

        private readonly IQueryable<T> _queryable;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
    }

}
