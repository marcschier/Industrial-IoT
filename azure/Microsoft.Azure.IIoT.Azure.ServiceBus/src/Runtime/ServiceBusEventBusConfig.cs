// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime {
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ServiceBus configuration
    /// </summary>
    public class ServiceBusEventBusConfig : ServiceBusConfig, IServiceBusEventBusConfig {

        private const string kTopicKey = "ServiceBus:Topic";

        /// <inheritdoc/>
        public string Topic => GetStringOrDefault(kTopicKey,
            () => null);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public ServiceBusEventBusConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
