// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Twin Event model for event bus
    /// </summary>
    public class TwinEventModel {

        /// <summary>
        /// Type of event
        /// </summary>
        public TwinEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Twin id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Twin info
        /// </summary>
        public TwinInfoModel Twin { get; set; }
    }
}