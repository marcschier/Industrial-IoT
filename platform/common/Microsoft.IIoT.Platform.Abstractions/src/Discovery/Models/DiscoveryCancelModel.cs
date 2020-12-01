﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {
    using Microsoft.IIoT.Platform.Core.Models;

    /// <summary>
    /// Cancel discovery
    /// </summary>
    public class DiscoveryCancelModel {

        /// <summary>
        /// Id of discovery request
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Operation audit context
        /// </summary>
        public OperationContextModel Context { get; set; }
    }
}
