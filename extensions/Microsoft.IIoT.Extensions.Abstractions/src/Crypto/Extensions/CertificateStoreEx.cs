// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto {
    using Microsoft.IIoT.Extensions.Crypto.Models;
    using Microsoft.IIoT.Exceptions;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Certificate store extensions
    /// </summary>
    public static class CertificateStoreEx {

        /// <summary>
        /// Get most recent certificate with the given name
        /// from certificate store.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="certificateName">Name of certificate
        /// </param>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        public static async Task<Certificate> GetLatestCertificateAsync(
            this ICertificateStore store, string certificateName,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            var result = await store.FindLatestCertificateAsync(certificateName, ct).ConfigureAwait(false);
            if (result == null) {
                throw new ResourceNotFoundException("Failed to find certificate");
            }
            return result;
        }

        /// <summary>
        /// Find certificate by serial number.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="serialNumber">serial number of the
        /// certificate </param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<Certificate> FindCertificateAsync(
            this ICertificateStore store, IReadOnlyCollection<byte> serialNumber,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            try {
                return await store.GetCertificateAsync(serialNumber, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Get certificate chain of trust.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="certificate">certificate</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Certificate>> ListCompleteCertificateChainAsync(
            this ICertificateStore store, Certificate certificate,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            var certificates = new List<Certificate>();
            var chain = await store.ListCertificateChainAsync(certificate, ct).ConfigureAwait(false);
            if (chain != null) {
                certificates.AddRange(chain.Certificates);
            }
            while (chain?.ContinuationToken != null) {
                chain = await store.ListCertificatesAsync(
                    chain.ContinuationToken, null, ct).ConfigureAwait(false);
                certificates.AddRange(chain.Certificates);
            }
            return certificates;
        }

        /// <summary>
        /// Get certificate chain of trust.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="serialNumber">serial number of the
        /// certificate </param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<CertificateCollection> ListCertificateChainAsync(
            this ICertificateStore store, byte[] serialNumber,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            var cert = await store.GetCertificateAsync(serialNumber, ct).ConfigureAwait(false);
            return await store.ListCertificateChainAsync(cert, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get certificate chain of trust.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="serialNumber">serial number of the
        /// certificate </param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Certificate>> ListCompleteCertificateChainAsync(
            this ICertificateStore store, IReadOnlyCollection<byte> serialNumber,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            var cert = await store.GetCertificateAsync(serialNumber, ct).ConfigureAwait(false);
            return await store.ListCompleteCertificateChainAsync(cert, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Query all certificates
        /// </summary>
        /// <param name="store"></param>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Certificate>> QueryAllCertificatesAsync(
            this ICertificateStore store, CertificateFilter filter,
            CancellationToken ct = default) {
            if (store is null) {
                throw new System.ArgumentNullException(nameof(store));
            }
            var results = await store.QueryCertificatesAsync(filter, null, ct).ConfigureAwait(false);
            var certificates = new List<Certificate>(results.Certificates);
            while (results.ContinuationToken != null) {
                results = await store.ListCertificatesAsync(
                    results.ContinuationToken, null, ct).ConfigureAwait(false);
                certificates.AddRange(results.Certificates);
            }
            return certificates;
        }
    }
}

