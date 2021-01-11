// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage.Services {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MemCache = System.Runtime.Caching.MemoryCache;

    /// <summary>
    /// In memory cache
    /// </summary>
    public sealed class MemoryCache : ICache {

        /// <inheritdoc/>
        public Task<byte[]> GetAsync(string key, CancellationToken ct) {
            return Task.FromResult((byte[])kCache.Get(key));
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key, CancellationToken ct) {
            kCache.Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetAsync(string key, byte[] value,
            DateTimeOffset expiration, CancellationToken ct) {
            kCache.Set(key, value, expiration);
            return Task.CompletedTask;
        }

        private static readonly MemCache kCache =
            new MemCache(typeof(MemoryCache).Name);
    }
}
