// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.AspNetCore.KeyVault.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Storage configuration
    /// </summary>
    internal sealed class StorageConfig : PostConfigureOptionBase<StorageOptions> {

        /// <inheritdoc/>
        public StorageConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, StorageOptions options) {
            if (string.IsNullOrEmpty(options.AccountName)) {
                options.AccountName = GetConnectonStringTokenOrDefault(
                    PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.Endpoint,
                    GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_ACCOUNT",
                    GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT")));
            }
            if (string.IsNullOrEmpty(options.EndpointSuffix)) {
                options.EndpointSuffix = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.EndpointSuffix,
                    GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX",
                    GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX",
                    "core.windows.net")));
            }
            if (string.IsNullOrEmpty(options.AccountKey)) {
                options.AccountKey = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.SharedAccessKey,
                    GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_KEY",
                    GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_KEY")));
            }
        }
    }
}
