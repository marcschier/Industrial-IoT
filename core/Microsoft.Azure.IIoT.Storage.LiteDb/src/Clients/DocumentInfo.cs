// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using LiteDB;

    /// <summary>
    /// Document wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DocumentInfo<T> : IDocumentInfo<T> {

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="bson"></param>
        internal DocumentInfo(BsonDocument bson, BsonMapper mapper = null) {
            Bson = bson ?? throw new ArgumentNullException(nameof(bson));
            _mapper = mapper ?? BsonMapper.Global;
        }

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="value"></param>
        /// <param name="id"></param>
        internal DocumentInfo(T value, string id = null, BsonMapper mapper = null) {
            _mapper = mapper ?? BsonMapper.Global;
            Bson = mapper.ToDocument(value);
            if (!string.IsNullOrEmpty(id)) {
                Bson[IdProperty] = id;
            }
            Bson[EtagProperty] = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public T Value => _mapper.Deserialize<T>(Bson);

        /// <inheritdoc/>
        public string Id => Bson[IdProperty];

        /// <inheritdoc/>
        public string PartitionKey => Bson[PartitionKeyProperty];

        /// <inheritdoc/>
        public string Etag => Bson[EtagProperty];

        /// <summary>
        /// Bson document
        /// </summary>
        public BsonDocument Bson { get; }

        private readonly BsonMapper _mapper;
        internal const string IdProperty = "id";
        internal const string PartitionKeyProperty = "pk";
        internal const string EtagProperty = "_etag";
    }
}
