﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Subscription status model
    /// </summary>
    public class SubscriptionStatusModel {

        /// <summary>
        /// Subscription from which message originated
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Last error
        /// </summary>
        public ServiceResultModel Error { get; set; }
    }
}