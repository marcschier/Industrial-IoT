// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {
    using Microsoft.IIoT.Platform.Core.Models;

    /// <summary>
    /// Dataset source state extensions
    /// </summary>
    public static class PublishedDataSetSourceStateModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublishedDataSetSourceStateModel Clone(
            this PublishedDataSetSourceStateModel model) {
            if (model?.LastResultChange == null &&
                model?.LastResult == null &&
                model?.ConnectionState == null) {
                return null;
            }
            return new PublishedDataSetSourceStateModel {
                ConnectionState = model.ConnectionState.Clone(),
                LastResultChange = model.LastResultChange,
                LastResult = model.LastResult.Clone()
            };
        }
    }
}