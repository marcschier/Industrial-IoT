﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage {
    using System;

    /// <summary>
    /// Container create options
    /// </summary>
    public class ContainerOptions {

        /// <summary>
        /// Item time to live
        /// </summary>
        public TimeSpan? ItemTimeToLive { get; set; }

        /// <summary>
        /// Parition key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Throughput units
        /// </summary>
        public int? ThroughputUnits { get; set; }
    }
}
