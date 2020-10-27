// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using System;

    /// <summary>
    /// Data value model
    /// </summary>
    public class DataValueModel{

        /// <summary>
        /// Value
        /// </summary>
        public VariantValue Value {get; set; }

        /// <summary>
        /// Data type of value
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Status of the value (Quality)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Source Timesamp
        /// </summary>
        public DateTime? SourceTimestamp { get; set; }

        /// <summary>
        /// Source pico
        /// </summary>
        public ushort? SourcePicoseconds { get; set; }

        /// <summary>
        /// Server Timestamp
        /// </summary>
        public DateTime? ServerTimestamp { get; set; }

        /// <summary>
        /// Server pico
        /// </summary>
        public ushort? ServerPicoseconds { get; set; }
    }
}