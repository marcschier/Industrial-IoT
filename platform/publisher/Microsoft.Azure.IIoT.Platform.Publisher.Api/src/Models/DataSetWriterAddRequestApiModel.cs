// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Data set writer registration request
    /// </summary>
    [DataContract]
    public class DataSetWriterAddRequestApiModel {

        /// <summary>
        /// Name of the published dataset
        /// </summary>
        [DataMember(Name = "dataSetName", Order = 0,
            EmitDefaultValue = false)]
        public string DataSetName { get; set; }

        /// <summary>
        /// Endpoint id to create dataset writer with
        /// </summary>
        [DataMember(Name = "endpointId", Order = 1)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Dataset writer group the writer is part of or default group.
        /// The writer group must have the same site as the endpoint.
        /// </summary>
        [DataMember(Name = "writerGroupId", Order = 2,
            EmitDefaultValue = false)]
        public string WriterGroupId { get; set; }

        /// <summary>
        /// User credentials to use
        /// </summary>
        [DataMember(Name = "user", Order = 3,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Dataset field content mask
        /// </summary>
        [DataMember(Name = "dataSetFieldContentMask", Order = 4,
            EmitDefaultValue = false)]
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Data set message settings
        /// </summary>
        [DataMember(Name = "messageSettings", Order = 5,
            EmitDefaultValue = false)]
        public DataSetWriterMessageSettingsApiModel MessageSettings { get; set; }

        /// <summary>
        /// Keyframe count
        /// </summary>
        [DataMember(Name = "keyFrameCount", Order = 6,
            EmitDefaultValue = false)]
        public uint? KeyFrameCount { get; set; }

        /// <summary>
        /// Or keyframe timer interval (publisher extension)
        /// </summary>
        [DataMember(Name = "keyFrameInterval", Order = 7,
            EmitDefaultValue = false)]
        public TimeSpan? KeyFrameInterval { get; set; }

        /// <summary>
        /// Extension fields in the dataset
        /// </summary>
        [DataMember(Name = "extensionFields", Order = 8,
            EmitDefaultValue = false)]
        public Dictionary<string, string> ExtensionFields { get; set; }

        /// <summary>
        /// Subscription settings (publisher extension)
        /// </summary>
        [DataMember(Name = "subscriptionSettings", Order = 9,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceSettingsApiModel SubscriptionSettings { get; set; }
    }
}
