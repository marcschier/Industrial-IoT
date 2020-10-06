// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Runtime {
    using Microsoft.Azure.IIoT.Authentication;
    using IdentityServer4;
    using IdentityServer4.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Identity server configuration
    /// </summary>
    public class IdentityServerConfig : IIdentityServerConfig {

        /// <summary>
        /// Clients
        /// </summary>
        public IEnumerable<Client> Clients { get; }

        /// <summary>
        /// Api resources
        /// </summary>
        public IEnumerable<ApiResource> Apis { get; }

        /// <summary>
        /// Identity resources
        /// </summary>
        public IEnumerable<IdentityResource> Ids { get; }

        /// <summary>
        /// Create configuration from configuration
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="servers"></param>
        public IdentityServerConfig(IClientAuthConfig clients,
            IServerAuthConfig servers) {

            Apis = servers?.JwtBearerProviders?
                .Where(s => s.Provider == AuthProvider.AuthService)
                .Select(ToServiceModel)
                .ToList();
            Clients = clients?.Providers?
                .Where(s => s.Provider == AuthProvider.AuthService)
                .Select(s => ToClient(s, servers?.JwtBearerProviders?
                    .Where(s => s.Provider == AuthProvider.AuthService)
                    .Select(s => s.Audience)))
                .ToList();
        }

        /// <summary>
        /// Convert client configuration to client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static Client ToClient(IOAuthClientConfig config,
            IEnumerable<string> scopes) {

            var client = new Client {
                ClientUri = "urn:iiot",
                ClientId = config.ClientId,
                ClientName = config.ClientId,
                AllowOfflineAccess = true,
                AllowedScopes = scopes?.ToList() ?? new List<string>(),
            };

            if (config is IOpenIdClientConfig openId) {
                var uri = openId.ClientUri?.TrimEnd('/');
                if (string.IsNullOrEmpty(uri)) {
                    throw new ArgumentException("Missing clienti uri", nameof(config));
                }
                client.ClientUri = uri;
                client.AllowedGrantTypes = GrantTypes.Hybrid;
                client.RedirectUris = new List<string> { $"{uri}/signin-oidc" };
                client.PostLogoutRedirectUris = new List<string>{ uri };
                client.FrontChannelLogoutUri = $"{uri}/signout-oidc";

                client.AllowedScopes.Add(IdentityServerConstants.StandardScopes.OpenId);
                client.AllowedScopes.Add(IdentityServerConstants.StandardScopes.Profile);
                client.AllowedScopes.Add(IdentityServerConstants.StandardScopes.Email);
            }

          // TODO  else if (config is IOpenApiClientConfig swagger) {
          // TODO      client.AllowedGrantTypes = GrantTypes.Implicit;
          // TODO      client.RedirectUris = swagger.RedirectUris ?? new List<string>();
          // TODO  }

            else {
                client.AllowedGrantTypes = GrantTypes.Implicit;
            }

            // Add secret
            if (!string.IsNullOrEmpty(config.ClientSecret)) {
                client.ClientSecrets = new List<Secret> {
                    new Secret(config.ClientSecret)
                };
            }
            return client;
        }

        /// <summary>
        /// Convert client configuration to client
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static ApiResource ToServiceModel(IOAuthServerConfig config) {
            var api = new ApiResource(config.Audience);

            // TODO: Add claims for roles, etc.

            return api;
        }
    }
}