// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Writer group event
    /// </summary>
    [DataContract]
    public class WriterGroupEventApiModel {

        /// <summary>
        /// Event type
        /// </summary>
        [DataMember(Name = "eventType", Order = 0)]
        public WriterGroupEventType EventType { get; set; }

        /// <summary>
        /// Writer group id
        /// </summary>
        [DataMember(Name = "id", Order = 1,
            EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Writer group
        /// </summary>
        [DataMember(Name = "writerGroup", Order = 2,
            EmitDefaultValue = false)]
        public WriterGroupInfoApiModel WriterGroup { get; set; }
    }
}