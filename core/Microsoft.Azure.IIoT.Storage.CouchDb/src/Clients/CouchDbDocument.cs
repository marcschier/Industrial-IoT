// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using CouchDB.Driver.Types;
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Document wrapper
    /// </summary>
    internal sealed class CouchDbDocument : CouchDocument, IDocumentInfo<JObject> {

        /// <inheritdoc/>
        [JsonIgnore]
        public JObject Value {
            get {
                if (_value is null) {
                    _value = JObject.FromObject(Document);
                }
                return _value;
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public string Etag {
            get {
                if (Document.TryGetValue(kEtagProperty, out var etag)) {
                    return (string)etag;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public override string Id {
            get {
                if (Document.TryGetValue(kIdProperty, out var id)) {
                    return (string)id;
                }
                return base.Id; 
            }
            set {
                base.Id = value;
                Document.Add(kIdProperty, value);
            }
        }

        /// <summary>
        /// Convert to typed document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal IDocumentInfo<T> ToDocumentInfo<T>() {
            return new TypedDocument<T>(this);
        }

        /// <inheritdoc/>
        [JsonExtensionData]
        public IDictionary<string, JToken> Document { get; set; }

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="value"></param>
        internal static CouchDbDocument Create<T>(T value) {
            return new CouchDbDocument {
                Document = JObject
                    .FromObject(value)
                    .ToObject<IDictionary<string, JToken>>()
            };
        }

        /// <summary>
        /// Create updated document
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        internal static CouchDbDocument CreateUpdated<T>(T value, string id) {
            var doc = Create(value);
            doc.Document[kEtagProperty] = Guid.NewGuid().ToString();
            doc.Id = id;
            return doc;
        }

        /// <summary>
        /// Typed version of document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class TypedDocument<T> : IDocumentInfo<T> {

            /// <summary>
            /// Typed doc
            /// </summary>
            /// <param name="doc"></param>
            public TypedDocument(CouchDbDocument doc) {
                _doc = doc;
            }

            /// <inheritdoc/>
            public string Id => _doc.Id;
            /// <inheritdoc/>
            public T Value => _doc.Value.ToObject<T>();
            /// <inheritdoc/>
            public string Etag => _doc.Etag;

            private readonly CouchDbDocument _doc;
        }

        private JObject _value;
        private const string kIdProperty = "_id";
        private const string kEtagProperty = "_etag";
    }
}
