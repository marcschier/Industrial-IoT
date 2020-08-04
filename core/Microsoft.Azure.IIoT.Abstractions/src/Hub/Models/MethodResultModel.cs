// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {

    /// <summary>
    /// Twin service method results model
    /// </summary>
    public class MethodResultModel {

        /// <summary>
        /// Status
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Response payload
        /// TODO: replace with variantvalue
        /// </summary>
        public string JsonPayload { get; set; }
    }
}
