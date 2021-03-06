﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Models {
    using IdentityServer4.Models;

    /// <summary>
    /// Convert model to document and back
    /// </summary>
    public static class GrantDocumentModelEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static PersistedGrant ToServiceModel(this GrantDocumentModel entity) {
            if (entity == null) {
                return null;
            }
            return new PersistedGrant {
                ClientId = entity.ClientId,
                CreationTime = entity.CreationTime,
                Data = entity.Data,
                Expiration = entity.Expiration,
                SessionId = entity.SessionId,
                ConsumedTime = entity.ConsumedTime,
                Description = entity.Description,
                Key = entity.Key,
                SubjectId = entity.SubjectId,
                Type = entity.Type
            };
        }

        /// <summary>
        /// Convert to document model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GrantDocumentModel ToDocumentModel(this PersistedGrant model) {
            if (model == null) {
                return null;
            }
            return new GrantDocumentModel {
                ClientId = model.ClientId,
                CreationTime = model.CreationTime,
                Data = model.Data,
                Expiration = model.Expiration,
                SessionId = model.SessionId,
                ConsumedTime = model.ConsumedTime,
                Description = model.Description,
                Key = model.Key,
                SubjectId = model.SubjectId,
                Type = model.Type
            };
        }
    }
}