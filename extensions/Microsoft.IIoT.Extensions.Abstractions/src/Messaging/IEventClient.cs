// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Send events
    /// </summary>
    public interface IEventClient {

        /// <summary>
        /// Send event to a target resource
        /// </summary>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SendEventAsync(string target, byte[] data,
            string contentType, string eventSchema,
            string contentEncoding, CancellationToken ct = default);

        /// <summary>
        /// Send batch of events to a target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="batch"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema,
            string contentEncoding, CancellationToken ct = default);

        /// <summary>
        /// Send with completion callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="token"></param>
        /// <param name="complete"></param>
        void SendEvent<T>(string target, byte[] data,
            string contentType, string eventSchema,
            string contentEncoding, T token, Action<T, Exception> complete);
    }
}
