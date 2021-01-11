// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Storage;
    using System;

    /// <summary>
    /// Document wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DocumentInfo<T> : IDocumentInfo<T> {

        /// <inheritdoc/>
        public string Id => (string)Document["id"];

        /// <inheritdoc/>
        public T Value => Document.ConvertTo<T>();

        /// <inheritdoc/>
        public string Etag => (string)Document["_etag"];

        /// <summary>
        /// Document
        /// </summary>
        internal VariantValue Document { get; }

        /// <summary>
        /// Create document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doc"></param>
        internal DocumentInfo(VariantValue doc, string id = null) {
            Document = doc ?? throw new ArgumentNullException(nameof(doc));
            if (!string.IsNullOrEmpty(id)) {
                Document["id"].AssignValue(id);
            }
        }
    }
}
