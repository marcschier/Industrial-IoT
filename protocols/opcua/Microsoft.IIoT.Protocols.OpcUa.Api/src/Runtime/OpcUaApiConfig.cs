// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Api.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// API configuration
    /// </summary>
    internal sealed class OpcUaApiConfig : PostConfigureOptionBase<OpcUaApiOptions> {

        /// <inheritdoc/>
        public OpcUaApiConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, OpcUaApiOptions options) {
            if (string.IsNullOrEmpty(options.OpcUaServiceUrl)) {
                options.OpcUaServiceUrl =
                    GetStringOrDefault(PcsVariable.PCS_OPCUA_SERVICE_URL,
                        GetDefaultUrl("9041", "twin"));
            }
        }
    }
}
