// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Method call response model
    /// </summary>
    [DataContract]
    public class MethodCallResponseApiModel {

        /// <summary>
        /// Output results
        /// </summary>
        [DataMember(Name = "results", Order = 0)]
        public List<MethodCallArgumentApiModel> Results { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        [DataMember(Name = "errorInfo", Order = 1,
            EmitDefaultValue = false)]
        public ServiceResultApiModel ErrorInfo { get; set; }
    }
}
