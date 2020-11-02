// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Datalake.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Datalake file storage configuration
    /// </summary>
    internal sealed class DatalakeConfig : ConfigBase<DatalakeOptions> {

        /// <inheritdoc/>
        public DatalakeConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, DatalakeOptions options) {

            options.AccountName = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.Endpoint,
                () => GetStringOrDefault(PcsVariable.PCS_ADLSG2_ACCOUNT,
                () => null));
            options.EndpointSuffix = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.EndpointSuffix,
                () => GetStringOrDefault(PcsVariable.PCS_ADLSG2_ENDPOINTSUFFIX,
                () => "dfs.core.windows.net"));
            options.AccountKey = GetConnectonStringTokenOrDefault(
                PcsVariable.PCS_ADLSG2_CONNSTRING, cs => cs.SharedAccessKey,
                () => GetStringOrDefault(PcsVariable.PCS_ADLSG2_ACCOUNT_KEY,
                () => null));
        }
    }
}
