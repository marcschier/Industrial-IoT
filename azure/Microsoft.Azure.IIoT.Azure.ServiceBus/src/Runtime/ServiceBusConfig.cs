// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ServiceBus configuration
    /// </summary>
    internal sealed class ServiceBusConfig : ConfigBase<ServiceBusOptions> {

        /// <inheritdoc/>
        public ServiceBusConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, ServiceBusOptions options) {
            options.ServiceBusConnString = GetStringOrDefault(PcsVariable.PCS_SERVICEBUS_CONNSTRING,
                () => GetStringOrDefault("_SB_CS", () => null));
        }
    }
}
