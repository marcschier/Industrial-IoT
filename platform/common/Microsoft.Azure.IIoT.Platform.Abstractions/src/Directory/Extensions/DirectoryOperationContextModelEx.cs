// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Models {
    using System;

    /// <summary>
    /// Operation extensions
    /// </summary>
    public static class DirectoryOperationContextModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DirectoryOperationContextModel Clone(
            this DirectoryOperationContextModel model) {
            model = model.Validate();
            return new DirectoryOperationContextModel {
                AuthorityId = model.AuthorityId,
                Time = model.Time
            };
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DirectoryOperationContextModel Validate(
            this DirectoryOperationContextModel context) {
            if (context == null) {
                context = new DirectoryOperationContextModel {
                    AuthorityId = null, // Should throw if configured
                    Time = DateTime.UtcNow
                };
            }
            return context;
        }
    }
}
