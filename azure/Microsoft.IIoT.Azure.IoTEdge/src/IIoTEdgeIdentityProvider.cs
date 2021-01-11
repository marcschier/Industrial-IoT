// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity services for the edge
    /// </summary>
    public interface IIoTEdgeIdentityProvider {

        /// <summary>
        /// Registers device with device id and returns
        /// the connection string.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ct"></param>
        /// <returns>Connection string of device</returns>
        Task<string> GetConnectionStringAsync(
            string deviceId, CancellationToken ct = default);
    }
}