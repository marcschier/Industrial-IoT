// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Storage.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System;
    using System.Linq;

    /// <summary>
    /// Twin (endpoint) document extensions
    /// </summary>
    public static class EndpointDocumentEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="document"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static EndpointInfoModel ToServiceModel(this EndpointDocument document,
            string etag) {
            if (document == null) {
                return null;
            }
            return new EndpointInfoModel {
                ApplicationId = document.ApplicationId,
                GenerationId = etag,
                Id = document.Id,
                DiscovererId = string.IsNullOrEmpty(document.DiscovererId) ?
                    null : document.DiscovererId,
                AuthenticationMethods = document.AuthenticationMethods?.ToList(),
                SecurityLevel = document.SecurityLevel,
                Endpoint = new EndpointModel {
                    Url = document.EndpointUrl,
                    AlternativeUrls = document.AlternativeUrls?.ToHashSetSafe(),
                    SecurityMode = document.SecurityMode == SecurityMode.Best ?
                        null : document.SecurityMode,
                    SecurityPolicy = string.IsNullOrEmpty(document.SecurityPolicy) ?
                        null : document.SecurityPolicy,
                    Certificate = document.Thumbprint
                },
                Updated = ToOperationModel(document.UpdateAuthorityId, document.UpdateTime),
                Created = ToOperationModel(document.CreateAuthorityId, document.CreateTime),
                Visibility = document.Visibility,
                NotSeenSince = document.NotSeenSince,
            };
        }

        /// <summary>
        /// Convert into document object
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointDocument ToDocumentModel(this EndpointInfoModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new EndpointDocument {
                Id = model.Id,
                Visibility = model.Visibility ?? EntityVisibility.Unknown,
                NotSeenSince = model.NotSeenSince,
                ApplicationId = model.ApplicationId,
                DiscovererId = model.DiscovererId,
                SecurityLevel = model.SecurityLevel,
                EndpointUrl = model.Endpoint.Url ?? string.Empty,
                AlternativeUrls = model.Endpoint.AlternativeUrls.ToHashSetSafe(),
                AuthenticationMethods = model.AuthenticationMethods?.ToList(),
                SecurityMode = model.Endpoint.SecurityMode ?? SecurityMode.Best,
                SecurityPolicy = model.Endpoint.SecurityPolicy ?? string.Empty,
                Thumbprint = model.Endpoint.Certificate ?? string.Empty,
                CreateAuthorityId = model.Created?.AuthorityId,
                CreateTime = model.Created?.Time ?? DateTime.UtcNow,
                UpdateAuthorityId = model.Updated?.AuthorityId,
                UpdateTime = model.Updated?.Time ?? DateTime.UtcNow,
            };
        }

        /// <summary>
        /// Create operation model
        /// </summary>
        /// <param name="authorityId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static OperationContextModel ToOperationModel(
            string authorityId, DateTime? time) {
            if (string.IsNullOrEmpty(authorityId) && time == null) {
                return null;
            }
            return new OperationContextModel {
                AuthorityId = authorityId,
                Time = time ?? DateTime.MinValue
            };
        }
    }
}
