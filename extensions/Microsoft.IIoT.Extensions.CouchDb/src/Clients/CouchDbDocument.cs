// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Clients {
    using Microsoft.IIoT.Storage;
    using CouchDB.Driver.Types;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Document wrapper
    /// </summary>
    internal sealed class CouchDbDocument : CouchDocument, IDocumentInfo<JToken> {

        /// <summary>
        /// The actual document values to serialize
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Document { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public JToken Value {
            get {
                if (_value is null) {
                    var o = JObject.FromObject(Document);

                    // Add etag and id as per convention
                    o.AddOrUpdate(kIdProperty, Id);
                    o.AddOrUpdate(kEtagProperty, Etag);

                    _value = o;
                }
                return _value;
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public string Etag {
            get {
                return Rev;
            }
            set {
                Rev = value;
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

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        /// <param name="etag"></param>
        internal static CouchDbDocument Wrap<T>(T value, string id, string etag) {
            var token = value is null ? JValue.CreateNull() : JToken.FromObject(value);
            if (token is JObject o) {
                return WrapJson(o, id, etag);
            }
            return new CouchDbDocument { _value = token };
        }

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="o"></param>
        /// <param name="id"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        internal static CouchDbDocument WrapJson(JObject o, string id, string etag) {
            var doc = new CouchDbDocument {
                Document = o.Properties().ToDictionary(p => p.Name, p => p.Value)
            };
            if (!string.IsNullOrWhiteSpace(id)) {
                doc.Id = id;
            }
            else if (doc.Document.TryGetValue(kIdProperty, out var jid)) {
                doc.Id = (string)jid;
            }
            else {
                doc.Id = Guid.NewGuid().ToString();
            }
            if (!string.IsNullOrWhiteSpace(etag)) {
                doc.Rev = etag;
            }
            else if (doc.Document.TryGetValue(kEtagProperty, out var jetag)) {
                doc.Rev = (string)jetag;
            }
            else {
                // new document - let database assign.
            }
            // Remove any occurrence of id to avoid duplication
            doc.Document.Remove(kIdProperty);
            doc.Document.Remove(kEtagProperty);
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

        private const string kIdProperty = "id";
        private const string kEtagProperty = "_etag";
        private JToken _value;
    }
}
