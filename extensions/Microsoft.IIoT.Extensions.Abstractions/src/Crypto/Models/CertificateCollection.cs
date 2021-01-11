// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Models {
    using System.Collections.Generic;

    /// <summary>
    /// A list of Certificates
    /// </summary>
    public class CertificateCollection {

        /// <summary>
        /// Certificate
        /// </summary>
        public IReadOnlyList<Certificate> Certificates { get; set; }

        /// <summary>
        /// Continuation token
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}

