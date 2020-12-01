// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.LiteDb.Clients {
    using Microsoft.IIoT.Storage;
    using System;
    using LiteDB;

    /// <summary>
    /// Document wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DocumentInfo<T> : IDocumentInfo<T> {

        /// <inheritdoc/>
        public T Value => _mapper.Deserialize<T>(_bson);

        /// <inheritdoc/>
        public string Id => _bson[kIdProperty];

        /// <inheritdoc/>
        public string Etag => _bson[kEtagProperty];

        /// <summary>
        /// Bson
        /// </summary>
        internal BsonDocument Bson => _bson.AsDocument;

        /// <summary>
        /// Register type
        /// </summary>
        static DocumentInfo() {
            DocumentSerializer.Register<T>();
        }

        /// <summary>
        /// Create document reading
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="bson"></param>
        internal DocumentInfo(BsonValue bson, BsonMapper mapper) {
            _bson = bson ?? throw new ArgumentNullException(nameof(bson));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="value"></param>
        internal DocumentInfo(T value, BsonMapper mapper) {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _bson = _mapper.Serialize(value);
        }

        /// <summary>
        /// Create updated document
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="value"></param>
        /// <param name="id"></param>
        internal DocumentInfo(T value, BsonMapper mapper, string id) :
            this(value, mapper) {
            if (_bson is BsonDocument doc) {
                if (!string.IsNullOrEmpty(id)) {
                    doc[kIdProperty] = id;
                }
                doc[kEtagProperty] = Guid.NewGuid().ToString();
            }
        }

        private readonly BsonMapper _mapper;
        private readonly BsonValue _bson;

        private const string kIdProperty = "_id";
        private const string kEtagProperty = "_etag";
    }
}
