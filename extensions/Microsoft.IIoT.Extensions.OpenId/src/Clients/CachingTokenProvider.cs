﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Authentication.Clients {
    using Microsoft.IIoT.Extensions.Authentication.Models;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Storage;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Caching token provider
    /// </summary>
    public class CachingTokenProvider : DefaultTokenProvider {

        /// <inheritdoc/>
        public CachingTokenProvider(IEnumerable<ITokenSource> tokenSources,
            ICache cache) : base(tokenSources) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc/>
        public override async Task<TokenResultModel> GetTokenForAsync(
            string resource, IEnumerable<string> scopes = null) {
            if (string.IsNullOrEmpty(resource)) {
                resource = Http.Resource.Platform;
            }
            var token = await Try.Async(() => GetTokenFromCacheAsync(resource, scopes)).ConfigureAwait(false);
            if (token == null) {
                token = await base.GetTokenForAsync(resource, scopes).ConfigureAwait(false);
                if (token != null && !token.Cached) {
                    await Try.Async(() => _cache.SetAsync(GetKey(resource),
                        Encoding.UTF8.GetBytes(token.RawToken), token.ExpiresOn)).ConfigureAwait(false);
                }
            }
            return token;
        }

        /// <inheritdoc/>
        public override async Task InvalidateAsync(string resource) {
            if (string.IsNullOrEmpty(resource)) {
                resource = Http.Resource.Platform;
            }
            await _cache.RemoveAsync(GetKey(resource)).ConfigureAwait(false);
            await base.InvalidateAsync(resource).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper to get token from cache
        /// </summary>
        /// <returns></returns>
        private async Task<TokenResultModel> GetTokenFromCacheAsync(string resource,
            IEnumerable<string> scopes) {
            var cached = await _cache.GetAsync(GetKey(resource)).ConfigureAwait(false);
            if (cached != null) {
                var token = JwtSecurityTokenEx.Parse(Encoding.UTF8.GetString(cached));
                if (token.ExpiresOn >= DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30)) {
                    if (scopes != null) {
                        // TODO: Check token has all scope is part of the token
                        if (!scopes.All(scope => string.IsNullOrEmpty(scope))) {
                            return null;
                        }
                    }
                    return token;
                }
            }
            return null;
        }

        /// <summary>
        /// Create key for resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private static string GetKey(string resource) {
            return resource + nameof(CachingTokenProvider);
        }

        private readonly ICache _cache;
    }
}
