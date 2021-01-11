// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// RabbitMq configuration
    /// </summary>
    internal sealed class RabbitMqConfig : PostConfigureOptionBase<RabbitMqOptions> {

        /// <inheritdoc/>
        public RabbitMqConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, RabbitMqOptions options) {
            if (string.IsNullOrEmpty(options.HostName)) {
                options.HostName =
                    GetStringOrDefault(PcsVariable.PCS_RABBITMQ_HOSTNAME,
                    GetStringOrDefault("_RABBITMQ_HOST", "localhost"));
            }
            if (string.IsNullOrEmpty(options.UserName)) {
                options.UserName =
                    GetStringOrDefault(PcsVariable.PCS_RABBITMQ_USERNAME, "user");
            }
            if (string.IsNullOrEmpty(options.Key)) {
                options.Key =
                    GetStringOrDefault(PcsVariable.PCS_RABBITMQ_KEY, "bitnami");
            }
            if (options.RoutingKey == null) {
                options.RoutingKey = string.Empty;
            }
        }
    }
}
