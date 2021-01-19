﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ActiveDirectory.Utils {
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.Identity.Client;
    using System.Threading.Tasks;

    /// <summary>
    /// Decorates a client with a cache to keep tokens
    /// </summary>
    public class MsalConfidentialClientDecorator :
        MsalClientApplicationDecorator<IConfidentialClientApplication> {

        /// <summary>
        /// Create token cache
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cache"></param>
        /// <param name="applicationKey"></param>
        /// <param name="userKey"></param>
        public MsalConfidentialClientDecorator(IConfidentialClientApplication client,
            ICache cache, string applicationKey, string userKey) : base(client, cache, userKey) {
            _appTokenCache = new MsalTokenCacheDecorator(cache,
                client.AppTokenCache, applicationKey);
        }

        /// <inheritdoc/>
        public override async Task ClearCacheAsync() {
            await _appTokenCache.ClearAsync().ConfigureAwait(false);
            await base.ClearCacheAsync().ConfigureAwait(false);
        }

        private readonly MsalTokenCacheDecorator _appTokenCache;
    }
}