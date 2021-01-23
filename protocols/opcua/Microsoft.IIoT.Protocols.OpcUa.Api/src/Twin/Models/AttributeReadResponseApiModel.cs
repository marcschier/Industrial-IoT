// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Runtime.Serialization;

    /// <summary>
    /// Attribute value read
    /// </summary>
    [DataContract]
    public class AttributeReadResponseApiModel {

        /// <summary>
        /// Attribute value
        /// </summary>
        [DataMember(Name = "value", Order = 0)]
        public VariantValue Value { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        [DataMember(Name = "errorInfo", Order = 1,
            EmitDefaultValue = false)]
        public ServiceResultApiModel ErrorInfo { get; set; }
    }
}
