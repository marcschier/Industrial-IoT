// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Service.Controllers {
    using Microsoft.IIoT.Platform.Vault.Service.Filters;
    using Microsoft.IIoT.Platform.Vault.Service.Models;
    using Microsoft.IIoT.Platform.Vault.Api.Models;
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using Microsoft.IIoT.Platform.Vault;
    using Microsoft.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Certificate services.
    /// </summary>
    [ExceptionsFilter]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/certificates")]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public sealed class CertificatesController : ControllerBase {

        /// <summary>
        /// Create the controller.
        /// </summary>
        /// <param name="services"></param>
        public CertificatesController(ICertificateAuthority services) {
            _services = services;
        }

        /// <summary>
        /// Get Issuer CA Certificate chain.
        /// </summary>
        /// <param name="serialNumber">the serial number of the
        /// Issuer CA Certificate</param>
        /// <returns>The Issuer CA certificate chain</returns>
        [HttpGet("{serialNumber}")]
        [AutoRestExtension(NextPageLinkName = "nextPageLink")]
        public async Task<X509CertificateChainApiModel> GetIssuerCertificateChainAsync(
            string serialNumber) {
            if (string.IsNullOrEmpty(serialNumber)) {
                throw new ArgumentNullException(nameof(serialNumber));
            }
            // Use service principal
            HttpContext.User = null; // TODO Set sp
            var result = await _services.GetIssuerCertificateChainAsync(serialNumber).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get Issuer CA CRL chain.
        /// </summary>
        /// <param name="serialNumber">the serial number of the Issuer
        /// CA Certificate</param>
        /// <returns>The Issuer CA CRL chain</returns>
        [HttpGet("{serialNumber}/crl")]
        public async Task<X509CrlChainApiModel> GetIssuerCrlChainAsync(
            string serialNumber) {
            if (string.IsNullOrEmpty(serialNumber)) {
                throw new ArgumentNullException(nameof(serialNumber));
            }
            // Use service principal
            HttpContext.User = null; // TODO Set sp
            var result = await _services.GetIssuerCrlChainAsync(serialNumber).ConfigureAwait(false);
            return result.ToApiModel();
        }

        private readonly ICertificateAuthority _services;
    }
}
