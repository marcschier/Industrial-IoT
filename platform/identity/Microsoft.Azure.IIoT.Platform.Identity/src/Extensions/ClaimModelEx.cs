﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Models {
    using IdentityServer4.Models;
    using System.Security.Claims;

    /// <summary>
    /// Convert model to document and back
    /// </summary>
    internal static class ClaimModelEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ClientClaim ToServiceModel(this ClaimModel entity) {
            if (entity == null) {
                return null;
            }
            return new ClientClaim(entity.Type, entity.Value, entity.ValueType);
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Claim ToClaim(this ClaimModel entity) {
            if (entity == null) {
                return null;
            }
            return new Claim(entity.Type, entity.Value, entity.ValueType);
        }

        /// <summary>
        /// Maps to document
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ClaimModel ToDocumentModel(this Claim entity) {
            if (entity == null) {
                return null;
            }
            return new ClaimModel {
                Type = entity.Type,
                ValueType = entity.ValueType,
                Value = entity.Value
            };
        }

        /// <summary>
        /// Maps to document
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ClaimModel ToDocumentModel(this ClientClaim entity) {
            if (entity == null) {
                return null;
            }
            return new ClaimModel {
                Type = entity.Type,
                ValueType = entity.ValueType,
                Value = entity.Value
            };
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ClaimModel Clone(this ClaimModel entity) {
            if (entity == null) {
                return null;
            }
            return new ClaimModel {
                Type = entity.Type,
                ValueType = entity.ValueType,
                Value = entity.Value
            };
        }
    }
}