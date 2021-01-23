// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Core.Models {
    using System;

    /// <summary>
    /// Operation context model
    /// </summary>
    public class OperationContextModel {

        /// <summary>
        /// User
        /// </summary>
        public string AuthorityId { get; set; }

        /// <summary>
        /// Operation time
        /// </summary>
        public DateTime Time { get; set; }
    }
}

