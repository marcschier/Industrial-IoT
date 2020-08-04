// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;

    /// <summary>
    /// Dataset writer event
    /// </summary>
    public class DataSetWriterEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public DataSetWriterEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public PublisherOperationContextModel Context { get; set; }

        /// <summary>
        /// Writer id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Writer
        /// </summary>
        public DataSetWriterInfoModel DataSetWriter { get; set; }
    }
}