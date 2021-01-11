// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Certificate factory configuration 
    /// </summary>
    public class CertificateFactoryConfig : PostConfigureOptionBase<CertificateFactoryOptions> {

        /// <inheritdoc/>
        public CertificateFactoryConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, CertificateFactoryOptions options) {
            if (string.IsNullOrEmpty(options.AuthorityCrlRootUrl)) {
                options.AuthorityCrlRootUrl = GetStringOrDefault("PCS_AUTHORITY_CRL_ROOT_URL");
            }
            if (string.IsNullOrEmpty(options.AuthorityInfoRootUrl)) {
                options.AuthorityInfoRootUrl = GetStringOrDefault("PCS_AUTHORITY_INFO_ROOT_URL");
            }
        }
    }
}
