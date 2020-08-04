// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data set source akin to a monitored item subscription.
    /// </summary>
    [DataContract]
    public class PublishedDataSetSourceApiModel {

        /// <summary>
        /// Either published data variables
        /// </summary>
        [DataMember(Name = "publishedVariables", Order = 0,
            EmitDefaultValue = false)]
        public PublishedDataItemsApiModel PublishedVariables { get; set; }

        /// <summary>
        /// Or published events data
        /// </summary>
        [DataMember(Name = "publishedEvents", Order = 1,
            EmitDefaultValue = false)]
        public PublishedDataSetEventsApiModel PublishedEvents { get; set; }

        /// <summary>
        /// Connection information (publisher extension)
        /// </summary>
        [DataMember(Name = "connection", Order = 2)]
        public ConnectionApiModel Connection { get; set; }

        /// <summary>
        /// Subscription settings (publisher extension)
        /// </summary>
        [DataMember(Name = "subscriptionSettings", Order = 3,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceSettingsApiModel SubscriptionSettings { get; set; }

        /// <summary>
        /// Source state
        /// </summary>
        [DataMember(Name = "state", Order = 4,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceStateApiModel State { get; set; }
    }
}