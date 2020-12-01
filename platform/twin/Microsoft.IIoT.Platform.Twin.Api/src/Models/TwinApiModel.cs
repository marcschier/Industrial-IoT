// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api.Models {
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin model
    /// </summary>
    [DataContract]
    public class TwinApiModel {

        /// <summary>
        /// Connection identifier.
        /// </summary>
        [DataMember(Name = "id", Order = 0)]
        public string Id { get; set; }

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        [DataMember(Name = "connection", Order = 1,
            EmitDefaultValue = false)]
        public ConnectionApiModel Connection { get; set; }

        /// <summary>
        /// The last state of this connection
        /// </summary>
        [DataMember(Name = "connectionState", Order = 2,
            EmitDefaultValue = false)]
        public ConnectionStateApiModel ConnectionState { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 3)]
        public OperationContextApiModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        [DataMember(Name = "updated", Order = 4,
            EmitDefaultValue = false)]
        public OperationContextApiModel Updated { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 5)]
        public string GenerationId { get; set; }
    }
}
