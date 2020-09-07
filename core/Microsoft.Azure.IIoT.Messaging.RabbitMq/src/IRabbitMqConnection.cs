// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Messaging.RabbitMq.Services {
    using RabbitMQ.Client;

    /// <summary>
    /// Represents a connection
    /// </summary>
    public interface IRabbitMqConnection {

        /// <summary>
        /// Get queue channel
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        IModel GetChannel(string target);
    }
}