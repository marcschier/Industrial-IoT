// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Generic;
    using LiteDB;

    /// <summary>
    /// Lite database
    /// </summary>
    internal sealed class DocumentDatabase : IDatabase {

        /// <summary>
        /// Creates database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="logger"></param>
        internal DocumentDatabase(ILiteDatabase db, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <inheritdoc/>
        public Task<IItemContainer> OpenContainerAsync(string id,
            ContainerOptions options) {
            if (string.IsNullOrEmpty(id)) {
                id = "default";
            }
            id = id.Replace('-', '_');
            var container = new DocumentCollection(id, _db, options, _logger);
            return Task.FromResult<IItemContainer>(container);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListContainersAsync(CancellationToken ct) {
            return Task.FromResult(_db.GetCollectionNames());
        }

        /// <inheritdoc/>
        public Task DeleteContainerAsync(string id) {
            _db.DropCollection(id);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() {
            _db.Dispose();
        }

        private readonly ILiteDatabase _db;
        private readonly ILogger _logger;
    }
}
