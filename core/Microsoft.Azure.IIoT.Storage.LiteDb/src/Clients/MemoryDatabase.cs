// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Default {
    using Microsoft.Azure.IIoT.Storage.LiteDb.Clients;
    using Serilog;

    /// <summary>
    /// Provides memory database.
    /// </summary>
    public sealed class MemoryDatabase : LiteDbClient {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="logger"></param>
        public MemoryDatabase(ILogger logger) : base(null, logger) {
        }
    }
}
