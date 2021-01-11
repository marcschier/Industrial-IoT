// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ActiveDirectory.Clients {
    using Microsoft.IIoT.Azure.ActiveDirectory.Utils;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Authentication.Models;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using Microsoft.Identity.Client;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Authenticate using device code
    /// </summary>
    public abstract class MsalPublicClientBase : ITokenClient {

        /// <summary>
        /// Create device code provider with callback
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        protected MsalPublicClientBase(IClientAuthConfig config, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Query(AuthProvider.AzureAD)
                .Select(config => (config, CreatePublicClientApplication(config)))
                .ToList();
        }

        /// <inheritdoc/>
        public bool Supports(string resource) {
            return _config.Any(c => c.config.Resource == resource);
        }

        /// <inheritdoc/>
        public async Task<TokenResultModel> GetTokenForAsync(string resource,
            IEnumerable<string> scopes) {
            var exceptions = new List<Exception>();
            foreach (var client in _config.Where(c => c.config.Resource == resource)) {
                var decorator = client.Item2;
                var config = client.config;

                // there should ever only be one account in the cache if we authenticated before...
                var accounts = await decorator.Client.GetAccountsAsync().ConfigureAwait(false);
                scopes = GetScopes(config, scopes);
                if (accounts.Any()) {
                    try {
                        // Attempt to get a token from the cache (or refresh it silently if needed)
                        var result = await decorator.Client.AcquireTokenSilent(
                            scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false);

                        return result.ToTokenResult();
                    }
                    catch (MsalUiRequiredException) {
                        // Expected if not in cache - continue down
                    }
                    catch (Exception ex) {
                        _logger.LogDebug(ex, "Failed to get token for {resource} from cache...",
                            resource);
                        exceptions.Add(ex);
                        continue;
                    }
                }
                try {
                    var token = await GetTokenAsync(decorator.Client, resource, scopes).ConfigureAwait(false);
                    if (token != null) {
                        _logger.LogInformation("Successfully acquired token for {resource} with {config}.",
                           resource, config.GetName());
                        return token;
                    }
                }
                catch (MsalException ex) {
                    _logger.LogDebug(ex, "Failed to get token for {resource} with {config} " +
                        "- error: {error}",
                        resource, config.GetName(), ex.ErrorCode);
                    exceptions.Add(ex);
                }
                catch (Exception e) {
                    _logger.LogDebug(e, "Failed to get token for {resource} with {config}.",
                        resource, config.GetName());
                    exceptions.Add(e);
                }
            }
            if (exceptions.Count != 0) {
                throw new AggregateException(exceptions);
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual Task InvalidateAsync(string resource) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get token using public client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resource"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        protected abstract Task<TokenResultModel> GetTokenAsync(
            IPublicClientApplication client, string resource, IEnumerable<string> scopes);

        /// <summary>
        /// Override configure
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected virtual PublicClientApplicationBuilder ConfigurePublicClientApplication(
            string clientId, PublicClientApplicationBuilder builder) {
            return builder;
        }

        /// <summary>
        /// Create public client
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private MsalClientApplicationDecorator<IPublicClientApplication> CreatePublicClientApplication(
            IOAuthClientConfig config) {
            var builder = PublicClientApplicationBuilder.Create(config.ClientId)
                .WithTenantId(config.TenantId);
            builder = ConfigurePublicClientApplication(config.ClientId, builder);
            return new MsalClientApplicationDecorator<IPublicClientApplication>(
                builder.Build(), new MemoryCache(), config.ClientId);
        }

        /// <summary>
        /// Get scopes
        /// </summary>
        /// <param name="config"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetScopes(IOAuthClientConfig config,
            IEnumerable<string> scopes) {
            var requestedScopes = new HashSet<string>();
            if (scopes != null) {
                foreach (var scope in scopes) {
                    requestedScopes.Add(scope);
                }
            }
            if (config.Audience != null) {
                requestedScopes.Add(config.Audience + "/.default");
            }
            return requestedScopes;
        }

        /// <summary> Logger </summary>
        protected readonly ILogger _logger;
        private readonly List<(IOAuthClientConfig config,
            MsalClientApplicationDecorator<IPublicClientApplication>)> _config;
    }
}
