// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery;
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
        public async Task<X509CertificateChainModel> GetCertificateAsync(
            string twin, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct).ConfigureAwait(false);
            return await _certificates.GetCertificateAsync(conn, ct).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _registry;
        private readonly ICertificateServices<ConnectionModel> _certificates;
    }
}
