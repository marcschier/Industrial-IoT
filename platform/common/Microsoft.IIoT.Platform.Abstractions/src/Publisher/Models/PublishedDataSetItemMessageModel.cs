// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Sample message
    /// </summary>
    public class PublishedDataSetItemMessageModel {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Variable identifier if variable
        /// </summary>
        public string VariableId { get; set; }

        /// <summary>
        /// Publisher's time stamp
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Sequence Number
        /// </summary>
        public uint? SequenceNumber { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public DataValueModel Value { get; set; }

        /// <summary>
        /// Extension information
        /// </summary>
        public IReadOnlyDictionary<string, string> Extensions { get; set; }
    }
}