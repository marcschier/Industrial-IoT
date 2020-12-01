// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// State model extensions
    /// </summary>
    public static class WriterGroupStateModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static WriterGroupStateModel Clone(this WriterGroupStateModel model) {
            if (model?.LastStateChange == null &&
                model?.LastState == null) {
                return null;
            }
            return new WriterGroupStateModel {
                LastState = model.LastState,
                LastStateChange = model.LastStateChange
            };
        }

    }
}