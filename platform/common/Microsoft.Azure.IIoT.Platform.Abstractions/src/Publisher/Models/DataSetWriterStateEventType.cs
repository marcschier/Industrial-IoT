// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// State event type
    /// </summary>
    public enum DataSetWriterStateEventType {

        /// <summary>
        /// Monitored item service result
        /// </summary>
        PublishedItem,

        /// <summary>
        /// Subscription service result
        /// </summary>
        Source
    }
}
