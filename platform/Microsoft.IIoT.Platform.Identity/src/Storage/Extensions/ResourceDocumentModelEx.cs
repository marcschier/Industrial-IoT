// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Models {
    using Microsoft.IIoT.Exceptions;
    using System.Linq;
    using IdentityServer4.Models;

    /// <summary>
    /// Convert model to document and back
    /// </summary>
    internal static class ResourceDocumentModelEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Resource ToServiceModel(this ResourceDocumentModel entity) {
            if (entity == null) {
                return null;
            }
            switch (entity.ResourceType) {
                case nameof(ApiResource):
                    return new ApiResource {
                        Description = entity.Description,
                        DisplayName = entity.DisplayName,
                        Enabled = entity.Enabled,
                        Name = entity.Name,
                        Scopes = entity.Scopes?.ToList(),
                        Properties = entity.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        AllowedAccessTokenSigningAlgorithms = entity.AllowedAccessTokenSigningAlgorithms?.ToList(),
                        ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
                        ApiSecrets = entity.ApiSecrets?.Select(s => s.ToServiceModel()).ToList(),
                        UserClaims = entity.UserClaims?.ToList()
                    };
                case nameof(IdentityResource):
                    return new IdentityResource {
                        Description = entity.Description,
                        DisplayName = entity.DisplayName,
                        Emphasize = entity.Emphasize,
                        Enabled = entity.Enabled,
                        Properties = entity.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        Name = entity.Name,
                        Required = entity.Required,
                        ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
                        UserClaims = entity.UserClaims?.ToList()
                    };
                case nameof(ApiScope):
                    return new ApiScope {
                        Description = entity.Description,
                        DisplayName = entity.DisplayName,
                        Emphasize = entity.Emphasize,
                        Enabled = entity.Enabled,
                        Properties = entity.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        Name = entity.Name,
                        Required = entity.Required,
                        ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
                        UserClaims = entity.UserClaims?.ToList()
                    };
                default:
                    throw new ResourceInvalidStateException(
                        $"Unknown resource type {entity.ResourceType}");
            }
        }

        /// <summary>
        /// Convert to document model
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ResourceDocumentModel ToDocumentModel(this Resource entity) {
            if (entity == null) {
                return null;
            }
            switch (entity) {
                case ApiResource api:
                    return new ResourceDocumentModel {
                        ResourceType = nameof(ApiResource),
                        Description = api.Description,
                        DisplayName = api.DisplayName,
                        Enabled = api.Enabled,
                        Id = api.Name?.ToLowerInvariant(),
                        Name = api.Name,
                        Scopes = api.Scopes?.Select(s => s?.ToLowerInvariant()).ToList(),
                        Properties = api.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        AllowedAccessTokenSigningAlgorithms = api.AllowedAccessTokenSigningAlgorithms?.ToList(),
                        ShowInDiscoveryDocument = api.ShowInDiscoveryDocument,
                        ApiSecrets = api.ApiSecrets?.Select(s => s.ToDocumentModel()).ToList(),
                        UserClaims = api.UserClaims?.ToList()
                    };
                case IdentityResource id:
                    return new ResourceDocumentModel {
                        ResourceType = nameof(IdentityResource),
                        Description = id.Description,
                        DisplayName = id.DisplayName,
                        Emphasize = id.Emphasize,
                        Enabled = id.Enabled,
                        Properties = entity.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        Id = id.Name?.ToLowerInvariant(),
                        Name = id.Name,
                        Required = id.Required,
                        ShowInDiscoveryDocument = id.ShowInDiscoveryDocument,
                        UserClaims = id.UserClaims?.ToList()
                    };
                case ApiScope scope:
                    return new ResourceDocumentModel {
                        ResourceType = nameof(ApiScope),
                        Description = scope.Description,
                        DisplayName = scope.DisplayName,
                        Emphasize = scope.Emphasize,
                        Enabled = scope.Enabled,
                        Properties = entity.Properties?.ToDictionary(k => k.Key, v => v.Value),
                        Name = scope.Name,
                        Id = scope.Name?.ToLowerInvariant(),
                        Required = scope.Required,
                        ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                        UserClaims = scope.UserClaims?.ToList()
                    };
                default:
                    throw new ResourceInvalidStateException(
                        $"Unknown resource type {entity.GetType()}");
            }
        }
    }
}