// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements certificate services as adapter through twin registry
    /// </summary>
    public sealed class CertificateServicesAdapter : ICertificateServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="certificates"></param>
        public CertificateServicesAdapter(ITwinRegistry registry,
            ICertificateServices<ConnectionModel> certificates) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainModel> GetEndpointCertificateAsync(
            string twin, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct).ConfigureAwait(false);
            return await _certificates.GetEndpointCertificateAsync(conn, ct).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _registry;
        private readonly ICertificateServices<ConnectionModel> _certificates;
    }
}
