// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Application document persisted and comparable
    /// </summary>
    public static class ApplicationDocumentEx {

        /// <summary>
        /// Decode tags and property into document object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        /// <returns></returns>
        public static ApplicationDocument ToDocumentModel(
            this ApplicationInfoModel model, bool? disabled = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new ApplicationDocument {
                IsDisabled = disabled,
                DiscovererId = model.DiscovererId,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames.Clone(),
                HostAddresses = model.HostAddresses.ToHashSetSafe(),
                ApplicationType = model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ProductUri = model.ProductUri,
                NotSeenSince = model.NotSeenSince,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                GatewayServerUri = model.GatewayServerUri,
                Capabilities = model.Capabilities.ToHashSetSafe(),
                DiscoveryUrls = model.DiscoveryUrls.ToHashSetSafe(),
                CreateAuthorityId = model.Created?.AuthorityId,
                CreateTime = model.Created?.Time,
                UpdateAuthorityId = model.Updated?.AuthorityId,
                UpdateTime = model.Updated?.Time,
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="document"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static ApplicationInfoModel ToServiceModel(this ApplicationDocument document,
            string etag) {
            if (document == null) {
                return null;
            }
            return new ApplicationInfoModel {
                ApplicationId = document.Id,
                ApplicationName = document.ApplicationName,
                Locale = document.Locale,
                LocalizedNames = document.LocalizedNames.Clone(),
                HostAddresses = document.HostAddresses.ToHashSetSafe(),
                NotSeenSince = document.NotSeenSince,
                ApplicationType = document.ApplicationType ?? ApplicationType.Server,
                ApplicationUri = string.IsNullOrEmpty(document.ApplicationUri) ?
                    document.ApplicationUriUC : document.ApplicationUri,
                ProductUri = document.ProductUri,
                GenerationId = etag,
                DiscovererId = string.IsNullOrEmpty(document.DiscovererId) ?
                    null : document.DiscovererId,
                DiscoveryUrls = document.DiscoveryUrls.ToHashSetSafe(),
                DiscoveryProfileUri = document.DiscoveryProfileUri,
                GatewayServerUri = document.GatewayServerUri,
                Capabilities = document.Capabilities.ToHashSetSafe(),
                Created = ToOperationModel(document.CreateAuthorityId, document.CreateTime),
                Updated = ToOperationModel(document.UpdateAuthorityId, document.UpdateTime),
            };
        }

        /// <summary>
        /// Create operation model
        /// </summary>
        /// <param name="authorityId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static RegistryOperationContextModel ToOperationModel(
            string authorityId, DateTime? time) {
            if (string.IsNullOrEmpty(authorityId) && time == null) {
                return null;
            }
            return new RegistryOperationContextModel {
                AuthorityId = authorityId,
                Time = time ?? DateTime.MinValue
            };
        }
    }
}
