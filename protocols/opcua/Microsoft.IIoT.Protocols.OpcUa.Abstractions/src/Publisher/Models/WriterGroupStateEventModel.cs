// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {

    /// <summary>
    /// Writer group state change event
    /// </summary>
    public class WriterGroupStateEventModel {

        /// <summary>
        /// Writer group id
        /// </summary>
        public string WriterGroupId { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public WriterGroupStateModel State { get; set; }
    }
}
