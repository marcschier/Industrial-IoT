// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Message queue client
    /// </summary>
    public interface IEventQueueClient {

        /// <summary>
        /// Send the provided message
        /// </summary>
        /// <param name="target"></param>
        /// <param name="payload"></param>
        /// <param name="properties"></param>
        /// <param name="partitionKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties = null,
            string partitionKey = null, CancellationToken ct = default);
    }
}
