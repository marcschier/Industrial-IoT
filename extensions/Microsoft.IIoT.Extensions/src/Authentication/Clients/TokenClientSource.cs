﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Authentication.Clients {
    using Microsoft.IIoT.Extensions.Authentication.Models;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Utils;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Use token client as token source
    /// </summary>
    public class TokenClientSource<T> : ITokenSource
        where T : ITokenClient {

        /// <inheritdoc/>
        public string Resource { get; } = Http.Resource.Platform;

        /// <inheritdoc/>
        public bool IsEnabled => _client.Supports(Resource);

        /// <inheritdoc/>
        public TokenClientSource(T client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<TokenResultModel> GetTokenAsync(
            IEnumerable<string> scopes = null) {
            return await Try.Async(() => _client.GetTokenForAsync(Resource, scopes)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task InvalidateAsync() {
            await Try.Async(() => _client.InvalidateAsync(Resource)).ConfigureAwait(false);
        }

        private readonly T _client;
    }
}