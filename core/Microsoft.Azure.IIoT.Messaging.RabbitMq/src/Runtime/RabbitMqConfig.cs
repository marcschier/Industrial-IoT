// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// RabbitMq configuration
    /// </summary>
    public class RabbitMqConfig : ConfigBase, IRabbitMqConfig {

        private const string kRabbitMqHostName = "RabbitMq:HostName";
        private const string kRabbitMqUserName = "RabbitMq:UserName";
        private const string kRabbitMqKey = "RabbitMq:Key";

        /// <inheritdoc/>
        public string HostName => GetStringOrDefault(kRabbitMqHostName,
            () => GetStringOrDefault(PcsVariable.PCS_RABBITMQ_HOSTNAME,
                () => GetStringOrDefault("_RABBITMQ_HOST", () => "localhost")));
        /// <inheritdoc/>
        public string UserName => GetStringOrDefault(kRabbitMqUserName,
            () => GetStringOrDefault(PcsVariable.PCS_RABBITMQ_USERNAME,
                () => "user"));
        /// <inheritdoc/>
        public string Key => GetStringOrDefault(kRabbitMqKey,
            () => GetStringOrDefault(PcsVariable.PCS_RABBITMQ_KEY,
                () => "bitnami"));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public RabbitMqConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
