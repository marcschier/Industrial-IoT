// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq {

    /// <summary>
    /// RabbitMq configuration
    /// </summary>
    public class RabbitMqOptions {

        /// <summary>
        /// Host name
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Routing key
        /// </summary>
        public string RoutingKey { get; set; }
    }
}
