﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto {
    using Microsoft.IIoT.Extensions.Crypto.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Certificate storage
    /// </summary>
    public interface ICertificateStore {

        /// <summary>
        /// Get most recent certificate with the given name
        /// from certificate store.
        /// </summary>
        /// <param name="certificateName">Name of certificate
        /// </param>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        Task<Certificate> FindLatestCertificateAsync(
            string certificateName, CancellationToken ct = default);

        /// <summary>
        /// Get certificate by serial number.
        /// </summary>
        /// <param name="serialNumber">serial number of the
        /// certificate </param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Certificate> GetCertificateAsync(
            IReadOnlyCollection<byte> serialNumber,
            CancellationToken ct = default);

        /// <summary>
        /// Get the chain of trust for a given certificate.
        /// The chain starts at the root and includes the
        /// certificate itself as last element.
        /// </summary>
        /// <param name="certificate">Certificate</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        Task<CertificateCollection> ListCertificateChainAsync(
            Certificate certificate, CancellationToken ct = default);

        /// <summary>
        /// Query certificates
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CertificateCollection> QueryCertificatesAsync(
            CertificateFilter filter, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Read all certificates or continue from continuation
        /// token returned by Trust chain or query query.
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CertificateCollection> ListCertificatesAsync(
            string continuationToken = null, int? pageSize = null,
            CancellationToken ct = default);
    }
}
