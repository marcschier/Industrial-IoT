// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ActiveDirectory.Runtime {
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Managed service identity configuration for storage
    /// </summary>
    public class MsiStorageClientConfig : ConfigureOptionBase, IOAuthClientConfig {

        /// <summary>
        /// Client configuration
        /// </summary>
        private const string kAuth_AppIdKey = "Msi:AppId";
        private const string kAuth_TenantIdKey = "Msi:TenantId";

        /// <summary>Audience</summary>
        public string Audience => "https://storage.azure.com";
        /// <summary>Resource</summary>
        public string Resource => Extensions.Http.Resource.Storage;
        /// <inheritdoc/>
        public bool IsValid => ClientId != null && TenantId != null;
        /// <inheritdoc/>
        public string Provider => AuthProvider.Msi;
        /// <inheritdoc/>
        public string ClientId => GetStringOrDefault(kAuth_AppIdKey,
            GetStringOrDefault(PcsVariable.PCS_MSI_APPID,
                null))?.Trim();
        /// <inheritdoc/>
        public string TenantId => GetStringOrDefault(kAuth_TenantIdKey,
            GetStringOrDefault(PcsVariable.PCS_MSI_TENANT,
                null))?.Trim();
        /// <inheritdoc/>
        public string InstanceUrl => null;
        /// <inheritdoc/>
        public string ClientSecret => null;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public MsiStorageClientConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
