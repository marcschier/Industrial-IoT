// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Runtime {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ServiceBus configuration
    /// </summary>
    internal sealed class ServiceBusConfig : PostConfigureOptionBase<ServiceBusOptions> {

        /// <inheritdoc/>
        public ServiceBusConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ServiceBusOptions options) {
            if (string.IsNullOrEmpty(options.ServiceBusConnString)) {
                options.ServiceBusConnString =
                    GetStringOrDefault(PcsVariable.PCS_SERVICEBUS_CONNSTRING,
                    GetStringOrDefault("_SB_CS"));
            }
        }
    }
}
