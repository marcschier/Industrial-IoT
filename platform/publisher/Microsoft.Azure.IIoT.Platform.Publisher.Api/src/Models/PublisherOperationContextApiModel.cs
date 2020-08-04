// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Publisher operation log model
    /// </summary>
    [DataContract]
    public class PublisherOperationContextApiModel {

        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "authorityId", Order = 0,
            EmitDefaultValue = false)]
        public string AuthorityId { get; set; }

        /// <summary>
        /// Operation time
        /// </summary>
        [DataMember(Name = "time", Order = 1,
            EmitDefaultValue = false)]
        public DateTime Time { get; set; }
    }
}

