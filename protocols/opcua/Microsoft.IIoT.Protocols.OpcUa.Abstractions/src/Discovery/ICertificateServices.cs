// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Certificate services for twin
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICertificateServices<T> {

        /// <summary>
        /// Get twin certificate
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<X509CertificateChainModel> GetCertificateAsync(T twin,
            CancellationToken ct = default);
    }
}
