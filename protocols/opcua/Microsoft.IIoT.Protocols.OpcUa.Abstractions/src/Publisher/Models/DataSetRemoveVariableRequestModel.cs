// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {
    /// <summary>
    /// A data set variable removal request
    /// </summary>
    public class DataSetRemoveVariableRequestModel {

        /// <summary>
        /// Node id of the variable(s) to unregister
        /// </summary>
        public string PublishedVariableNodeId { get; set; }
    }
}