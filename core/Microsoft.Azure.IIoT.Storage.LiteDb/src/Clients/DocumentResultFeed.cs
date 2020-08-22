// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using LiteDB;

    /// <summary>
    /// Wraps a document query to return statements
    /// </summary>
    internal sealed class DocumentResultFeed<T> : IResultFeed<T> {

        /// <inheritdoc/>
        public string ContinuationToken { get; private set; }

        /// <summary>
        /// Create feed
        /// </summary>
        internal DocumentResultFeed(ILiteQueryableResult<T> query, ILogger logger) {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ReadAsync(CancellationToken ct) {
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                if (_query.HasMoreResults) {
                    try {
                        var result = await _query.ExecuteNextAsync<T>(ct);
                        ContinuationToken = result.ResponseContinuation;
                        return result;
                    }
                    catch (Exception ex) {
                        DocumentCollection.FilterException(ex);
                    }
                }
                return Enumerable.Empty<T>();
            });
        }

        /// <inheritdoc/>
        public bool HasMore() {
            return _query.HasMoreResults;
        }

        /// <summary>
        /// Dispose query
        /// </summary>
        public void Dispose() {
            _query.Dispose();
        }

        private readonly ILiteQueryableResult<T> _query;
        private readonly ILogger _logger;
    }
}
