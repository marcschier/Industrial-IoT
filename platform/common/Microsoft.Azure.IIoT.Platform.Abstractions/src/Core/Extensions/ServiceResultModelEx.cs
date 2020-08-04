// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    /// <summary>
    /// Service result extensions
    /// </summary>
    public static class ServiceResultModelEx {

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