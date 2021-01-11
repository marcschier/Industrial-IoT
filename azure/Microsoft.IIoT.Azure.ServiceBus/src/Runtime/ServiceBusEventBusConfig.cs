// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ServiceBus configuration
    /// </summary>
    internal sealed class ServiceBusEventBusConfig : PostConfigureOptionBase<ServiceBusEventBusOptions> {

        /// <inheritdoc/>
        public ServiceBusEventBusConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ServiceBusEventBusOptions options) {
            if (string.IsNullOrEmpty(options.Topic)) {
                options.Topic = GetStringOrDefault("PCS_SERVICEBUS_TOPIC");
            }
        }
    }
}
