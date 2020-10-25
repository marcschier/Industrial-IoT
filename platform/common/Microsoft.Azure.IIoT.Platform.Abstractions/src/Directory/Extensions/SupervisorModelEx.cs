// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Service model extensions for discovery service
    /// </summary>
    public static class SupervisorModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<SupervisorModel> model,
            IEnumerable<SupervisorModel> that) {
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
        public static bool IsSameAs(this SupervisorModel model,
            SupervisorModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            return that.Id == model.Id;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorModel Clone(this SupervisorModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorModel {
                Connected = model.Connected,
                Id = model.Id,
                OutOfSync = model.OutOfSync,
                LogLevel = model.LogLevel
            };
        }
    }
}