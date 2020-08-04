// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Certificate list
    /// </summary>
    [DataContract]
    public sealed class X509CertificateListApiModel {

        /// <summary>
        /// Certificates
        /// </summary>
        [DataMember(Name = "certificates", Order = 0,
            EmitDefaultValue = false)]
        public List<X509CertificateApiModel> Certificates { get; set; }

        /// <summary>
        /// Next link
        /// </summary>
        [DataMember(Name = "nextPageLink", Order = 1,
            EmitDefaultValue = false)]
        public string NextPageLink { get; set; }
    }
}
