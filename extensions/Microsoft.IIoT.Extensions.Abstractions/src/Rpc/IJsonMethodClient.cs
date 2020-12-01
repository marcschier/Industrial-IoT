// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Rpc {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Json payload device method call client as implemented by
    /// IoTHub service clients and IoTEdge Module clients.
    /// </summary>
    public interface IJsonMethodClient {

        /// <summary>
        /// Max payload size in bytes.
        /// </summary>
        int MaxMethodPayloadSizeInBytes { get; }

        /// <summary>
        /// Call a method on a target with json payload.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="method"></param>
        /// <param name="json"></param>
        /// <param name="timeout"></param>
        /// <param name="ct"></param>
        /// <returns>response json payload</returns>
        Task<string> CallMethodAsync(string target,
            string method, string json, TimeSpan? timeout = null,
            CancellationToken ct = default);
    }
}
