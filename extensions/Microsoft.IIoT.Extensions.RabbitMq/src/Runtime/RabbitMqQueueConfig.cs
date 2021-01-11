// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Runtime {
    using Microsoft.IIoT.Extensions.RabbitMq;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// RabbitMq configuration
    /// </summary>
    internal sealed class RabbitMqQueueConfig : PostConfigureOptionBase<RabbitMqQueueOptions> {

        /// <inheritdoc/>
        public RabbitMqQueueConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, RabbitMqQueueOptions options) {
            if (string.IsNullOrEmpty(options.Queue)) {
                options.Queue = GetStringOrDefault("PCS_RABBITMQ_QUEUE", "default");
            }
        }
    }
}
