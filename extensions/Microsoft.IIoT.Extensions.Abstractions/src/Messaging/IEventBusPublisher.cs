// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Messaging {
    using System.Threading.Tasks;

    /// <summary>
    /// Event bus publisher
    /// </summary>
    public interface IEventBusPublisher {

        /// <summary>
        /// Publish message to subscribers
        /// </summary>
        /// <param name="message"></param>
        Task PublishAsync<T>(T message);
    }
}
