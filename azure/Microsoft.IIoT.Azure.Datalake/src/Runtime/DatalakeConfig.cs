// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.Datalake.Runtime {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Datalake file storage configuration
    /// </summary>
    internal sealed class DatalakeConfig : PostConfigureOptionBase<DatalakeOptions> {

        /// <inheritdoc/>
        public DatalakeConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, DatalakeOptions options) {
            if (string.IsNullOrEmpty(options.AccountName)) {
                options.AccountName = GetConnectonStringTokenOrDefault(
                    PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.Endpoint,
                    GetStringOrDefault(PcsVariable.PCS_ADLSG2_ACCOUNT));
            }
            if (string.IsNullOrEmpty(options.EndpointSuffix)) {
                options.EndpointSuffix = GetConnectonStringTokenOrDefault(
                    PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.EndpointSuffix,
                    GetStringOrDefault(PcsVariable.PCS_ADLSG2_ENDPOINTSUFFIX,
                        "dfs.core.windows.net"));
            }
            if (string.IsNullOrEmpty(options.AccountKey)) {
                options.AccountKey = GetConnectonStringTokenOrDefault(
                    PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.SharedAccessKey,
                    GetStringOrDefault(PcsVariable.PCS_ADLSG2_ACCOUNT_KEY));
            }
        }
    }
}
