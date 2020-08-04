// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Result of a event dataset registration
    /// </summary>
    [DataContract]
    public class DataSetAddEventResponseApiModel {

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        public string GenerationId { get; set; }
    }
}