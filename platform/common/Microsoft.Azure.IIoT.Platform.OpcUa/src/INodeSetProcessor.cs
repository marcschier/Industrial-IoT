// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa {
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Process nodeset from stream
    /// </summary>
    public interface INodeSetProcessor {

        /// <summary>
        /// Process model from file
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ProcessAsync(Stream stream, INodeHandler handler,
            string contentType, CancellationToken ct = default);
    }
}