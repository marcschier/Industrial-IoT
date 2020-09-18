// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime {
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ServiceBus configuration
    /// </summary>
    public class ServiceBusProcessorConfig : ServiceBusConfig, IServiceBusProcessorConfig {

        private const string kQueueKey = "ServiceBus:Queue";

        /// <inheritdoc/>
        public string Queue => GetStringOrDefault(kQueueKey,
            () => GetStringOrDefault(PcsVariable.PCS_SERVICEBUS_QUEUE,
                () => null));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public ServiceBusProcessorConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
