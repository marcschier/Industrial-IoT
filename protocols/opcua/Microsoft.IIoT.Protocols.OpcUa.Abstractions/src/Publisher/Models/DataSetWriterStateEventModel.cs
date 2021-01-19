// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Dataset writer state change
    /// </summary>
    public class DataSetWriterStateEventModel {

        /// <summary>
        /// Dataset writer id if dataset related or null
        /// </summary>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Variable id if variable related or null
        /// </summary>
        public string PublishedVariableId { get; set; }

        /// <summary>
        /// Type of event
        /// </summary>
        public DataSetWriterStateEventType EventType { get; set; }

        /// <summary>
        /// Timestamp of event
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Result if event is result of service call
        /// </summary>
        public ServiceResultModel LastResult { get; set; }

        /// <summary>
        /// Connection state if state changed
        /// </summary>
        public ConnectionStateModel ConnectionState { get; set; }
    }
}
