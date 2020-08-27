// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.Documents.Linq;
    using Serilog;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DocumentQuery<T> : IQuery<T> {

        internal DocumentQuery(IQueryable<T> orderedQueryable, ILogger logger) {
            _queryable = orderedQueryable;
            _logger = logger;
        }

        public IResultFeed<IDocumentInfo<T>> GetResults() {
            return new DocumentInfoFeed<T, T>(_queryable.AsDocumentQuery(), _logger);
        }

        public Task<int> CountAsync(CancellationToken ct) {
            return _queryable.CountAsync();
        }

        public IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1) {
            return new DocumentQuery<T>(_queryable.OrderBy(keySelector), _logger);
        }

        public IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector) {
            return new DocumentQuery<T>(_queryable.OrderByDescending(keySelector), _logger);
        }

        public IQuery<K> Select<K>(Expression<Func<T, K>> selector) {
            return new DocumentQuery<K>(_queryable.Select(selector), _logger);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate) {
            return new DocumentQuery<T>(_queryable.Where(predicate), _logger);
        }

        public IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector) {
            return new DocumentQuery<K>(_queryable.SelectMany(selector), _logger);
        }

        private readonly IQueryable<T> _queryable;
        private readonly ILogger _logger;
    }

}
