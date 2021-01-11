// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.IIoT.Extensions.Crypto {

    /// <summary>
    /// Issuer configuration
    /// </summary>
    public class CertificateFactoryOptions {

        /// <summary>
        /// Crl Distribution point template
        /// </summary>
        public string AuthorityCrlRootUrl { get; set; }

        /// <summary>
        /// Authority information access template
        /// </summary>
        public string AuthorityInfoRootUrl { get; set; }
    }
}
