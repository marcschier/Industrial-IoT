// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Core.Models {
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Authentication method model extensions
    /// </summary>
    public static class AuthenticationMethodModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<AuthenticationMethodModel> model,
            IEnumerable<AuthenticationMethodModel> that) {
            if (model == that) {
                return true;
            }
            return model.SetEqualsSafe(that, (x, y) => x.IsSameAs(y));
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this AuthenticationMethodModel model,
            AuthenticationMethodModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (model.Configuration != null && that.Configuration != null) {
                if (!VariantValue.DeepEquals(model.Configuration, that.Configuration)) {
                    return false;
                }
            }
            return
                model.Id == that.Id &&
                model.SecurityPolicy == that.SecurityPolicy &&
                model.CredentialType == that.CredentialType;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationMethodModel Clone(this AuthenticationMethodModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMethodModel {
                Configuration = model.Configuration?.Copy(),
                Id = model.Id,
                SecurityPolicy = model.SecurityPolicy,
                CredentialType = model.CredentialType
            };
        }
    }
}
