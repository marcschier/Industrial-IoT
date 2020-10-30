// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Certificate request model
    /// </summary>
    public sealed class CertificateRequestModel {

        /// <summary>
        /// Public record
        /// </summary>
        public CertificateRequestRecordModel Record { get; set; }

        /// <summary>
        /// Entity info
        /// </summary>
        public EntityInfoModel Entity { get; set; }

        /// <summary>
        /// Signing request
        /// </summary>
        public IReadOnlyCollection<byte> SigningRequest { get; set; }

        /// <summary>
        /// Resulting certificate
        /// </summary>
        public X509CertificateModel Certificate { get; set; }

        /// <summary>
        /// Optional private key handle
        /// </summary>
        public IReadOnlyCollection<byte> KeyHandle { get; set; }
    }
}
