// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Events.v2.Models {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Core.Models;

    /// <summary>
    /// Publisher event
    /// </summary>
    public class PublisherEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public PublisherEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Publisher id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Publisher
        /// </summary>
        public PublisherModel Publisher { get; set; }
    }
}