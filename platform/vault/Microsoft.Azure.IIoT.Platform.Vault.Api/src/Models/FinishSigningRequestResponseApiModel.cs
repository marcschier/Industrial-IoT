// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Finish request results
    /// </summary>
    [DataContract]
    public sealed class FinishSigningRequestResponseApiModel {

        /// <summary>
        /// Request
        /// </summary>
        [DataMember(Name = "request", Order = 0,
            EmitDefaultValue = false)]
        public CertificateRequestRecordApiModel Request { get; set; }

        /// <summary>
        /// Signed certificate
        /// </summary>
        [DataMember(Name = "certificate", Order = 1,
            EmitDefaultValue = false)]
        public X509CertificateApiModel Certificate { get; set; }
    }
}
