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
    internal sealed class ServiceBusProcessorConfig : PostConfigureOptionBase<ServiceBusProcessorOptions> {

        /// <inheritdoc/>
        public ServiceBusProcessorConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ServiceBusProcessorOptions options) {
            if (string.IsNullOrEmpty(options.Queue)) {
                options.Queue = GetStringOrDefault(PcsVariable.PCS_SERVICEBUS_QUEUE);
            }
        }
    }
}
