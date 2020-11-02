// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Storage configuration
    /// </summary>
    internal sealed class StorageConfig : ConfigBase<StorageOptions> {

        /// <inheritdoc/>
        public StorageConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, StorageOptions options) {

            options.AccountName = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.Endpoint,
                () => GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_ACCOUNT",
                () => GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT",
                () => null)));
            options.EndpointSuffix = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.EndpointSuffix,
                () => GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX",
                () => GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX",
                () => "core.windows.net")));
            options.AccountKey = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_STORAGE_CONNSTRING, cs => cs.SharedAccessKey,
                () => GetStringOrDefault("PCS_ASA_DATA_AZUREBLOB_KEY",
                () => GetStringOrDefault("PCS_IOTHUBREACT_AZUREBLOB_KEY",
                () => null)));
        }
    }
}
