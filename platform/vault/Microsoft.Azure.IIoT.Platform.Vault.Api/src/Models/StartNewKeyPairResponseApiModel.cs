// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// New key pair response
    /// </summary>
    [DataContract]
    public sealed class StartNewKeyPairRequestResponseApiModel {

        /// <summary>
        /// Request id
        /// </summary>
        [DataMember(Name = "requestId", Order = 0)]
        public string RequestId { get; set; }
    }
}
