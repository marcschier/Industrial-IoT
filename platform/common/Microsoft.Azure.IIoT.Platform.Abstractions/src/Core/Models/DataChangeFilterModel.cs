// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;

    /// <summary>
    /// Data change filter
    /// </summary>
    public class DataChangeFilterModel {

        /// <summary>
        /// Data change trigger type
        /// </summary>
        public DataChangeTriggerType? DataChangeTrigger { get; set; }

        /// <summary>
        /// Dead band
        /// </summary>
        public DeadbandType? DeadBandType { get; set; }

        /// <summary>
        /// Dead band value
        /// </summary>
        public double? DeadBandValue { get; set; }
    }
}