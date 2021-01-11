// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Sign payloads
    /// </summary>
    public interface IIoTEdgeDigestSigner {

        /// <summary>
        /// Sign digest uzsing primary key
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<byte[]> SignAsync(byte[] payload,
            CancellationToken ct = default);
    }
}