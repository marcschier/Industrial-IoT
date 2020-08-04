// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Data set source akin to a monitored item subscription.
    /// </summary>
    public class PublishedDataSetSourceModel {

        /// <summary>
        /// Either published data variables
        /// </summary>
        public PublishedDataItemsModel PublishedVariables { get; set; }

        /// <summary>
        /// Or published events data
        /// </summary>
        public PublishedDataSetEventsModel PublishedEvents { get; set; }

        /// <summary>
        /// Connection information (publisher extension)
        /// </summary>
        public ConnectionModel Connection { get; set; }

        /// <summary>
        /// Subscription settings (publisher extension)
        /// </summary>
        public PublishedDataSetSourceSettingsModel SubscriptionSettings { get; set; }

        /// <summary>
        /// Source state
        /// </summary>
        public PublishedDataSetSourceStateModel State { get; set; }
    }
}