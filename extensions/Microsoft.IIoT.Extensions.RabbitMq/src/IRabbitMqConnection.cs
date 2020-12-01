// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq {

    /// <summary>
    /// Represents a connection
    /// </summary>
    public interface IRabbitMqConnection {

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="consumer"></param>
        /// <param name="fanout"></param>
        /// <returns></returns>
        IRabbitMqChannel GetChannel(string name,
            IRabbitMqConsumer consumer = null,
            bool fanout = false);
    }
}