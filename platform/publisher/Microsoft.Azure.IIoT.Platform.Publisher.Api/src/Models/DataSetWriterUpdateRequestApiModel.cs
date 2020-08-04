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
    /// Data set writer registration update request
    /// </summary>
    [DataContract]
    public class DataSetWriterUpdateRequestApiModel {

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        public string GenerationId { get; set; }

        /// <summary>
        /// New Name of the published dataset
        /// </summary>
        [DataMember(Name = "dataSetName", Order = 1,
            EmitDefaultValue = false)]
        public string DataSetName { get; set; }

        /// <summary>
        /// Dataset writer group the writer should move to.
        /// The writer group must have the same site as the endpoint
        /// the writer was registered on.
        /// </summary>
        [DataMember(Name = "writerGroupId", Order = 2,
            EmitDefaultValue = false)]
        public string WriterGroupId { get; set; }

        /// <summary>
        /// New user credentials to use - set empty
        /// credential model to remove current.
        /// </summary>
        [DataMember(Name = "user", Order = 3,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Updated content mask or 0 to set back to default.
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
