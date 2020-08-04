// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;

    /// <summary>
    /// Model extensions for twin module
    /// </summary>
    public static class RegistryExtensions {

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static AuthenticationMethodApiModel ToApiModel(
            this AuthenticationMethodModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMethodApiModel {
                Id = model.Id,
                SecurityPolicy = model.SecurityPolicy,
                Configuration = model.Configuration,
                CredentialType = (IIoT.Platform.Core.Api.Models.CredentialType?)model.CredentialType ??
                    IIoT.Platform.Core.Api.Models.CredentialType.None
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static AuthenticationMethodModel ToServiceModel(
            this AuthenticationMethodApiModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMethodModel {
                Id = model.Id,
                SecurityPolicy = model.SecurityPolicy,
                Configuration = model.Configuration,
                CredentialType = (IIoT.Platform.Core.Models.CredentialType?)model.CredentialType ??
                    IIoT.Platform.Core.Models.CredentialType.None
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static EntityActivationStatusApiModel ToApiModel(
            this EntityActivationStatusModel model) {
            if (model == null) {
                return null;
            }
            return new EntityActivationStatusApiModel {
                Id = model.Id,
                ActivationState = (IIoT.Platform.Registry.Api.Models.EntityActivationState?)model.ActivationState
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static EntityActivationStatusModel ToServiceModel(
            this EntityActivationStatusApiModel model) {
            if (model == null) {
                return null;
            }
            return new EntityActivationStatusModel {
                Id = model.Id,
                ActivationState = (IIoT.Platform.Registry.Models.EntityActivationState?)model.ActivationState
            };
        }
    }
}
