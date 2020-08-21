// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using System;

    /// <summary>
    /// Document wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DocumentInfo<T> : IDocumentInfo<T> {

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="doc"></param>
        internal DocumentInfo(object doc) {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        }

        internal static DocumentInfo<S> Create<S>(dynamic doc) {
            return new DocumentInfo<S>(doc);
        }

        /// <inheritdoc/>
        public string Id => _doc.Id;

        /// <inheritdoc/>
        public T Value => (T)_doc;

        /// <inheritdoc/>
        public string PartitionKey => _doc.GetPropertyValue<string>(
            DocumentCollection.PartitionKeyProperty);

        /// <inheritdoc/>
        public string Etag => _doc.ETag;

        private readonly dynamic _doc;
    }
}
