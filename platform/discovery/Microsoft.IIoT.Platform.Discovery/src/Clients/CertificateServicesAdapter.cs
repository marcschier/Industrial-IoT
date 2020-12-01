// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Clients {
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements certificate services as adapter through endpoint registry
    /// </summary>
    public sealed class CertificateServicesAdapter : ICertificateServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="certificates"></param>
        public CertificateServicesAdapter(IEndpointRegistry registry,
            ICertificateServices<EndpointModel> certificates) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainModel> GetCertificateAsync(string endpoint, 
            CancellationToken ct) {
            var ep = await _registry.GetEndpointAsync(endpoint, ct).ConfigureAwait(false);
            return await _certificates.GetCertificateAsync(
                ep.Endpoint, ct).ConfigureAwait(false);
        }

        private readonly IEndpointRegistry _registry;
        private readonly ICertificateServices<EndpointModel> _certificates;
    }
}
