// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using System.IO;

    /// <summary>
    /// Wraps a document query to return document infos
    /// </summary>
    internal sealed class DocumentInfoFeed<T> : IResultFeed<IDocumentInfo<T>> {

        /// <inheritdoc/>
        public string ContinuationToken { get; private set; }

        /// <summary>
        /// Create feed
        /// </summary>
        internal DocumentInfoFeed(FeedIterator query, ISerializer serializer, ILogger logger) {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IDocumentInfo<T>>> ReadAsync(CancellationToken ct) {
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                if (_query.HasMoreResults) {
                    try {
                        var result = await _query.ReadNextAsync(ct).ConfigureAwait(false);
                        result.EnsureSuccessStatusCode();

                        ContinuationToken = result.ContinuationToken;
                        var items = _serializer.Parse(await result.Content.ReadAsMemoryAsync().ConfigureAwait(false));
                        return items["Documents"].Values
                            .Select(v => (IDocumentInfo<T>)new DocumentInfo<T>(v));
                    }
                    catch (Exception ex) {
                        DocumentCollection.FilterException(ex);
                    }
                }
                return Enumerable.Empty<IDocumentInfo<T>>();
            }).ConfigureAwait(false);
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

        private readonly FeedIterator _query;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
    }
}
