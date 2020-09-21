// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq {

    /// <summary>
    /// RabbitMq configuration
    /// </summary>
    public interface IRabbitMqConfig {

        /// <summary>
        /// Host name
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// User name
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Secret
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Routing key
        /// </summary>
        string RoutingKey { get; }
    }
}
