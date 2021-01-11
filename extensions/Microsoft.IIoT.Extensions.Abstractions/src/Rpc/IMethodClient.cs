// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Rpc {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Call method with buffer and content type as payload
    /// </summary>
    public interface IMethodClient {

        /// <summary>
        /// Call method on target with buffer and return payload
        /// as byte buffer.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="method"></param>
        /// <param name="payload"></param>
        /// <param name="contentType"></param>
        /// <param name="timeout"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<byte[]> CallMethodAsync(string target,
            string method, byte[] payload, string contentType,
            TimeSpan? timeout = null, CancellationToken ct = default);
    }
}
