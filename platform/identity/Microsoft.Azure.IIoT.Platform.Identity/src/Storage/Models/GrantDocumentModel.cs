// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Models {
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A model for a persisted grant
    /// </summary>
    [DataContract]
    public class GrantDocumentModel {

        /// <summary>
        /// Unique ID of the client
        /// </summary>
        [DataMember(Name = "id")]
        public string Key { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        [DataMember]
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        [DataMember]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        [DataMember]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        [DataMember]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [DataMember]
        public string Data { get; set; }

        /// <summary>
        /// Consumed time
        /// </summary>
        [DataMember]
        public DateTime? ConsumedTime { get; internal set; }

        /// <summary>
        /// Description to show
        /// </summary>
        [DataMember]
        public string Description { get; internal set; }

        /// <summary>
        /// Sets time to live for this grant
        /// </summary>
        [DataMember(Name = "ttl")]
        public long? TimeToLive =>
            Expiration == null ? (long?)null :
            Math.Max(1,
                (long)(DateTime.UtcNow - Expiration.Value)
                .TotalSeconds);

    }
}