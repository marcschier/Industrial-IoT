// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Http.Exceptions;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Serilog;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;

    /// <summary>
    /// Wraps a cosmos db container
    /// </summary>
    internal sealed class DocumentCollection : IItemContainer {

        /// <inheritdoc/>
        public string Name => _container.Id;


        /// <summary>
        /// Create collection
        /// </summary>
        /// <param name="partitioned"></param>
        /// <param name="serializer"></param>
        /// <param name="container"></param>
        /// <param name="logger"></param>
        internal DocumentCollection(Container container, ISerializer serializer, bool partitioned,
            ILogger logger) {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _partitioned = partitioned;
        }

        /// <inheritdoc/>
        public IQuery<T> CreateQuery<T>(int? pageSize, OperationOptions options) {
            var query = _container.GetItemLinqQueryable<T>(false, null,
                new QueryRequestOptions {
                    MaxItemCount = pageSize,
                    // ConsistencyLevel,
                    // PartitionKey =
                    //  EnableScanInQuery = true
                });
            return new DocumentQuery<T>(query, _serializer, false, _logger);
        }

        /// <inheritdoc/>
        public IResultFeed<IDocumentInfo<T>> ContinueQuery<T>(string continuationToken,
            int? pageSize, string partitionKey) {
            if (string.IsNullOrEmpty(continuationToken)) {
                throw new ArgumentNullException(nameof(continuationToken));
            }
            if (!continuationToken.Contains("\"Continuation\":")) {
                throw new BadRequestException(nameof(continuationToken));
            }
            var query = _container.GetItemLinqQueryable<T>(false, continuationToken,
                new QueryRequestOptions {
                    MaxItemCount = pageSize,
                    // ConsistencyLevel,
                    // PartitionKey = partitionKey
                    //  EnableScanInQuery = true
                });
            return new DocumentInfoFeed<T>(query.ToStreamIterator(), _serializer, _logger);
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> FindAsync<T>(string id, OperationOptions options,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            try {
                return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                    try {
                        var doc = await _container.ReadItemStreamAsync(id,
                            partitionKey: PartitionKey.None /*TODO*/, null, ct).ConfigureAwait(false);
                        doc.EnsureSuccessStatusCode();
                        return AsDocumentInfo<T>(doc.Content);
                    }
                    catch (Exception ex) {
                        FilterException(ex);
                        return null;
                    }
                }).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> UpsertAsync<T>(T newItem,
            string id, OperationOptions options, string etag, CancellationToken ct) {
            if (newItem == null) {
                throw new ArgumentNullException(nameof(newItem));
            }
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                try {
                    var doc = await _container.UpsertItemStreamAsync(AsStream(newItem, id),
                        partitionKey: PartitionKey.None /*TODO*/,
                        new ItemRequestOptions {
                            IfMatchEtag = etag,
                            EnableContentResponseOnWrite = true
                        }, ct).ConfigureAwait(false);
                    doc.EnsureSuccessStatusCode();
                    return AsDocumentInfo<T>(doc.Content);
                }
                catch (Exception ex) {
                    FilterException(ex);
                    return null;
                }
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> ReplaceAsync<T>(IDocumentInfo<T> existing,
            T newItem, OperationOptions options, CancellationToken ct) {
            if (existing == null) {
                throw new ArgumentNullException(nameof(existing));
            }
            if (string.IsNullOrEmpty(existing.Id)) {
                throw new ArgumentException("Missing id", nameof(existing));
            }
            if (newItem == null) {
                throw new ArgumentNullException(nameof(newItem));
            }
            options ??= new OperationOptions();
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                try {
                    var doc = await _container.ReplaceItemStreamAsync(
                        AsStream(newItem, existing.Id), existing.Id,
                        partitionKey: PartitionKey.None /*TODO*/,
                        new ItemRequestOptions {
                            IfMatchEtag = existing.Etag,
                            EnableContentResponseOnWrite = true
                        }, ct).ConfigureAwait(false);
                    doc.EnsureSuccessStatusCode();
                    return AsDocumentInfo<T>(doc.Content);
                }
                catch (Exception ex) {
                    FilterException(ex);
                    return null;
                }
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IDocumentInfo<T>> AddAsync<T>(T newItem, string id,
            OperationOptions options, CancellationToken ct) {
            if (newItem == null) {
                throw new ArgumentNullException(nameof(newItem));
            }
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                try {
                    var doc = await _container.CreateItemStreamAsync(AsStream(newItem, id),
                        partitionKey: PartitionKey.None /*TODO*/,
                        new ItemRequestOptions {
                            EnableContentResponseOnWrite = true
                        }, ct).ConfigureAwait(false);
                    doc.EnsureSuccessStatusCode();
                    return new DocumentInfo<T>(_serializer.Parse(doc.Content.ReadAsBuffer()));
                }
                catch (Exception ex) {
                    FilterException(ex);
                    return null;
                }
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task DeleteAsync<T>(IDocumentInfo<T> item, OperationOptions options,
            CancellationToken ct) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            options ??= new OperationOptions();
            return DeleteAsync<T>(item.Id, options, item.Etag, ct);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(string id, OperationOptions options,
            string etag, CancellationToken ct) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            await Retry.WithExponentialBackoff(_logger, ct, async () => {
                try {
                    var doc = await _container.DeleteItemStreamAsync(id,
                        partitionKey: PartitionKey.None /*TODO*/,
                        new ItemRequestOptions { IfMatchEtag = etag, }, ct).ConfigureAwait(false);
                    doc.EnsureSuccessStatusCode();
                }
                catch (Exception ex) {
                    FilterException(ex);
                    return;
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Convert to document info
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        private IDocumentInfo<T> AsDocumentInfo<T>(Stream stream) {
            return new DocumentInfo<T>(_serializer.Parse(stream.ReadAsBuffer()));
        }

        /// <summary>
        /// Convert to stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private Stream AsStream<T>(T item, string id = null) {
            var newDoc = new DocumentInfo<T>(_serializer.FromObject(item), id).Document;
            return new MemoryStream(_serializer.SerializeToBytes(newDoc).ToArray());
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
            else if (ex is CosmosException dce) {
                if (dce.StatusCode == (HttpStatusCode)429) {
                    throw new TemporarilyBusyException(dce.Message, dce, dce.RetryAfter);
                }
                dce.StatusCode.Validate(dce.Message, dce);
            }
            else {
                throw ex;
            }
        }

        private readonly ILogger _logger;
        private readonly bool _partitioned;
        private readonly Container _container;
        private readonly ISerializer _serializer;
    }
}
