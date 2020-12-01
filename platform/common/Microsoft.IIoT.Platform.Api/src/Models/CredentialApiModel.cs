// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Api.Models {
    using Microsoft.IIoT.Serializers;
    using System.Runtime.Serialization;

    /// <summary>
    /// Credential model
    /// </summary>
    [DataContract]
    public class CredentialApiModel {

        /// <summary>
        /// Type of credential
        /// </summary>
        [DataMember(Name = "type", Order = 0,
            EmitDefaultValue = false)]
        public CredentialType? Type { get; set; }

        /// <summary>
        /// Value to pass to server
        /// </summary>
        [DataMember(Name = "value", Order = 1,
            EmitDefaultValue = false)]
        public VariantValue Value { get; set; }
    }
}
