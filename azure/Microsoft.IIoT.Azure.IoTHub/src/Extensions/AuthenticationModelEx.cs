// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;

    /// <summary>
    /// Authentication model extensions
    /// </summary>
    internal static class AuthenticationModelEx {

        /// <summary>
        /// Convert twin to module
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationMechanism ToAuthenticationMechanism(
            this AuthenticationModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMechanism {
                SymmetricKey = new SymmetricKey {
                    PrimaryKey = model.PrimaryKey,
                    SecondaryKey = model.SecondaryKey
                }
            };
        }

        /// <summary>
        /// Convert twin to module
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationModel ToAuthenticationModel(
            this AuthenticationMechanism model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationModel {
                PrimaryKey = model.SymmetricKey.PrimaryKey,
                SecondaryKey = model.SymmetricKey.SecondaryKey
            };
        }

        /// <summary>
        /// Clone authentication
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationModel Clone(this AuthenticationModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationModel {
                PrimaryKey = model.PrimaryKey,
                SecondaryKey = model.SecondaryKey
            };
        }
    }
}
