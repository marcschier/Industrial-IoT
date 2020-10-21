// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    /// <summary>
    /// Connection state model extensions
    /// </summary>
    public static class ConnectionStateModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this ConnectionStateModel model, ConnectionStateModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (!that.LastResult.IsSameAs(model.LastResult)) {
                return false;
            }
            if (that.State != model.State) {
                return false;
            }
            if (that.LastResultChange != model.LastResultChange) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConnectionStateModel Clone(this ConnectionStateModel model) {
            if (model == null) {
                return null;
            }
            return new ConnectionStateModel {
                LastResult = model.LastResult.Clone(),
                LastResultChange = model.LastResultChange,
                State = model.State
            };
        }

    }
}