// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Services {
    using Microsoft.Azure.IIoT.Diagnostics;
    using System;
    using Xunit;

    [CollectionDefinition(Name)]
    public class CouchDbServerCollection : ICollectionFixture<CouchDbServerFixture> {

        public const string Name = "Server";
    }

    public class CouchDbServerFixture : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public CouchDbServerFixture() {
            try {
                _server = new CouchDbServer(ConsoleLogger.Create());
                _server.StartAsync().GetAwaiter().GetResult();
            }
            catch (Exception) {
                _server = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _server?.Dispose();
        }

        private readonly CouchDbServer _server;
    }
}