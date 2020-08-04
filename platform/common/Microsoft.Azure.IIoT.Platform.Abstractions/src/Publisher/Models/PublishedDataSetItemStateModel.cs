// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// State of the dataset item
    /// </summary>
    public class PublishedDataSetItemStateModel {

        /// <summary>
        /// Last operation result
        /// </summary>
        public ServiceResultModel LastResult { get; set; }

        /// <summary>
        /// Last result change
        /// </summary>
        public DateTime? LastResultChange { get; set; }

        /// <summary>
        /// Assigned server identifier
        /// </summary>
        public uint? ServerId { get; set; }

        /// <summary>
        /// Assigned client identifier
        /// </summary>
        public uint? ClientId { get; set; }
    }
}