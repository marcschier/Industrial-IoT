// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using System;

    /// <summary>
    /// A monitored and published variable
    /// </summary>
    public class PublishedItemModel {

        /// <summary>
        /// Variable node id to monitor
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Display name of the variable node monitored
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Sampling interval to use
        /// </summary>
        public TimeSpan? SamplingInterval { get; set; }

        /// <summary>
        /// Heartbeat interval to use
        /// </summary>
        public TimeSpan? HeartbeatInterval { get; set; }

        /// <summary>
        /// Publishing interval to use
        /// </summary>
        public TimeSpan? PublishingInterval { get; set; }
    }
}
