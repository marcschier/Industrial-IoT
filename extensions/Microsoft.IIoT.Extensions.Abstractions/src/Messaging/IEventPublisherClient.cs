// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Messaging {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event publisher client
    /// </summary>
    public interface IEventPublisherClient {

        /// <summary>
        /// Send the provided message
        /// </summary>
        /// <param name="target"></param>
        /// <param name="payload"></param>
        /// <param name="properties"></param>
        /// <param name="partitionKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishAsync(string target, byte[] payload,
            IDictionary<string, string> properties = null,
            string partitionKey = null, CancellationToken ct = default);

        /// <summary>
        /// Send the provided messages
        /// </summary>
        /// <param name="target"></param>
        /// <param name="batch"></param>
        /// <param name="properties"></param>
        /// <param name="partitionKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishAsync(string target, IEnumerable<byte[]> batch,
            IDictionary<string, string> properties = null,
            string partitionKey = null, CancellationToken ct = default);

        /// <summary>
        /// Publish with callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <param name="complete"></param>
        /// <param name="properties"></param>
        /// <param name="partitionKey"></param>
        void Publish<T>(string target, byte[] payload,
            T token, Action<T, Exception> complete,
            IDictionary<string, string> properties = null,
            string partitionKey = null);
    }
}
