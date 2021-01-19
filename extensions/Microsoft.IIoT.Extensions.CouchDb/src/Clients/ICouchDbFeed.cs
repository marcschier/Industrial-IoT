﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Clients {
    using Microsoft.IIoT.Extensions.Storage;

    /// <summary>
    /// Feed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ICouchDbFeed<T> : IResultFeed<IDocumentInfo<T>> {

        /// <inheritdoc/>
        int? PageSize { get; set; }
    }
}