// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.RabbitMq.Services {

    /// <summary>
    /// Queue consumer configuration
    /// </summary>
    public interface IRabbitMqQueueConfig {

        /// <summary>
        /// Queue to consume from
        /// </summary>
        string Queue { get; }
    }
}