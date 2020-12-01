// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Models {
    /// <summary>
    /// Service result extensions
    /// </summary>
    public static class ServiceResultModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this ServiceResultModel model, ServiceResultModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (that.StatusCode != model.StatusCode) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ServiceResultModel Clone(this ServiceResultModel model) {
            if (model == null) {
                return null;
            }
            return new ServiceResultModel {
                Diagnostics = model.Diagnostics?.Copy(),
                ErrorMessage = model.ErrorMessage,
                StatusCode = model.StatusCode,
            };
        }
    }
}