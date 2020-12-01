// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Authentication.Clients.Default {
    using Microsoft.IIoT.Authentication.Models;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Linq;
    using global::IdentityModel.Client;

    /// <summary>
    /// Authenticate using client credentials
    /// </summary>
    public sealed class ClientCredentialClient : ITokenClient {

        /// <summary>
        /// Http client factory
        /// </summary>
        public IHttpClientFactory Http { get; set; }

        /// <summary>
        /// Create console output device code based token provider
        /// </summary>
        /// <param name="config"></param>
        /// <param name="http"></param>
        /// <param name="logger"></param>
        public ClientCredentialClient(IClientAuthConfig config, ILogger logger,
            IHttpClientFactory http = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Http = http ?? new HttpClientFactory(logger);
        }

        /// <inheritdoc/>
        public bool Supports(string resource) {
            return _config.Query(resource, AuthProvider.AuthService).Any();
        }

        /// <inheritdoc/>
        public async Task<TokenResultModel> GetTokenForAsync(string resource,
            IEnumerable<string> scopes) {
            var exceptions = new List<Exception>();
            foreach (var config in _config.Query(resource, AuthProvider.AuthService)) {
                if (string.IsNullOrEmpty(config.ClientSecret)) {
                    continue;
                }
                try {
                    var client = Http.CreateClient("token_client");
                    using var request = new ClientCredentialsTokenRequest {
                        Address = $"{config.GetAuthority()}/connect/token",
                        ClientId = config.ClientId,
                        ClientSecret = config.ClientSecret
                    };
                    var response = await client.RequestClientCredentialsTokenAsync(
                        request).ConfigureAwait(false);
                    if (response.IsError) {
                        _logger.LogError("Error {error} aquiring token for {resource} with {config}",
                            response.Error, resource, config.GetName());
                        return null;
                    }
                    var result = JwtSecurityTokenEx.Parse(response.AccessToken);
                    _logger.LogInformation(
                        "Successfully acquired token for {resource} with {config}.",
                        resource, config.GetName());
                    return result;
                }
                catch (Exception exc) {
                    _logger.LogDebug(exc, "Failed to get token for {resource} using {config}",
                        resource, config.GetName());
                    exceptions.Add(exc);
                }
            }
            if (exceptions.Count != 0) {
                throw new AggregateException(exceptions);
            }
            return null;
        }

        /// <inheritdoc/>
        public Task InvalidateAsync(string resource) {
            return Task.CompletedTask;
        }

        private readonly ILogger _logger;
        private readonly IClientAuthConfig _config;
    }
}
