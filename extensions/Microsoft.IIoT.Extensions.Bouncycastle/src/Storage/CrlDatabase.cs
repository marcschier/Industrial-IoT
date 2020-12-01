// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Crypto.Storage {
    using Microsoft.IIoT.Crypto.Models;
    using Microsoft.IIoT.Crypto.Storage.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Storage;
    using Microsoft.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Crl database acting as a cache and endpoint for crl objects.
    /// </summary>
    public sealed class CrlDatabase : ICrlEndpoint, ICrlRepository, IHostProcess, IDisposable {

        /// <summary>
        /// Create database
        /// </summary>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        /// <param name="certificates"></param>
        /// <param name="logger"></param>
        public CrlDatabase(ICollectionFactory container, ICertificateStore certificates,
            ICrlFactory factory, ILogger logger) {
            if (container is null) {
                throw new ArgumentNullException(nameof(container));
            }

            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _crls = container.OpenAsync("crls").Result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Crl>> GetCrlChainAsync(IReadOnlyCollection<byte> serial,
            CancellationToken ct) {
            var serialNumber = new SerialNumber(serial);
            var document = await TryGetOrAddCrlAsync(serialNumber, TimeSpan.Zero, ct).ConfigureAwait(false);
            if (document == null) {
                throw new ResourceNotFoundException("Cert for serial number not found.");
            }

            var result = new List<Crl>();
            var model = document.ToModel();
            if (model != null) {
                result.Add(model);
            }

            // Walk the chain - compare subject and issuer serial number
            while (!string.IsNullOrEmpty(document.IssuerSerialNumber) &&
                !document.IssuerSerialNumber.EqualsIgnoreCase(serialNumber.ToString())) {

                serialNumber = SerialNumber.Parse(document.IssuerSerialNumber);
                document = await TryGetOrAddCrlAsync(serialNumber, TimeSpan.Zero, ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Cert chain is broken.");
                }
                model = document.ToModel();
                if (model == null) {
                    throw new ResourceNotFoundException("Crl chain is broken.");
                }
                result.Add(model);
            }
            // Reverse so to have root first
            result.Reverse();
            return result;
        }


        /// <inheritdoc/>
        public async Task InvalidateAsync(IReadOnlyCollection<byte> serial, CancellationToken ct) {
            var serialNumber = new SerialNumber(serial).ToString();
            await Try.Async(() => _crls.DeleteAsync<CrlDocument>(serialNumber, ct: ct)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_cacheManager != null) {
                    _logger.LogDebug("Cache manager host already running.");
                }
                _logger.LogDebug("Starting cache manager host...");
                _cts = new CancellationTokenSource();
                _cacheManager = Task.Run(() => ManageCacheAsync(_cts.Token));
                _logger.LogInformation("Cache manager host started.");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start cache manager host.");
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                _cts?.Cancel();
                if (_cacheManager != null) {
                    _logger.LogDebug("Stopping cache manager host...");
                    await Try.Async(() => _cacheManager).ConfigureAwait(false);
                    _logger.LogInformation("Cache manager host stopped.");
                }
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to stop cache manager host.");
            }
            finally {
                _cacheManager = null;
                _cts?.Dispose();
                _cts = null;

                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
            _lock.Dispose();
            _cts?.Dispose();
        }

        /// <summary>
        /// Get or add crl to cache database
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="validatyPeriod"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<CrlDocument> TryGetOrAddCrlAsync(SerialNumber serialNumber,
            TimeSpan validatyPeriod, CancellationToken ct) {
            if (serialNumber is null) {
                throw new ArgumentNullException(nameof(serialNumber));
            }

            while (true) {
                var crl = await _crls.FindAsync<CrlDocument>(serialNumber.ToString(), ct: ct).ConfigureAwait(false);
                if (crl != null &&
                    crl.Value.NextUpdate > (DateTime.UtcNow - validatyPeriod)) {
                    return crl.Value;
                }

                // Find issuer certificate.
                var issuer = await _certificates.FindCertificateAsync(serialNumber.Value, ct).ConfigureAwait(false);
                if (issuer?.IssuerPolicies == null || issuer.Revoked != null) {
                    if (crl != null) {
                        // Get rid of crl
                        await Try.Async(() => _crls.DeleteAsync(crl, ct: ct)).ConfigureAwait(false);
                    }
                    if (issuer == null) {
                        return null;  // Unknown certificate
                    }
                    // Not an issuer cert
                    return new CrlDocument {
                        IssuerSerialNumber = issuer.GetIssuerSerialNumberAsString(),
                        SerialNumber = issuer.GetSerialNumberAsString(),
                    };
                }

                // Get all revoked but still valid certificates issued by issuer
                var revoked = await _certificates.GetIssuedCertificatesAsync(
                    issuer, null, true, true, ct).ConfigureAwait(false);
                System.Diagnostics.Debug.Assert(revoked.All(r => r.Revoked != null));

                // Build crl

                var result = await _factory.CreateCrlAsync(issuer,
                    issuer.IssuerPolicies.SignatureType.Value, revoked, null, ct).ConfigureAwait(false);
                var document = result.ToDocument(
                    issuer.GetSerialNumberAsString(), issuer.GetIssuerSerialNumberAsString());
                try {
                    // Add crl
                    crl = await _crls.UpsertAsync(document, null, null, crl?.Etag, ct).ConfigureAwait(false);
                    return crl.Value;
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <summary>
        /// Manage cache
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ManageCacheAsync(CancellationToken ct) {
            while (!ct.IsCancellationRequested) {
                try {
                    // Get all issuer certificates
                    var issuers = await _certificates.QueryCertificatesAsync(
                        new CertificateFilter {
                            IsIssuer = true
                        }, null, ct).ConfigureAwait(false);

                    while (!ct.IsCancellationRequested) {
                        foreach (var issuer in issuers.Certificates) {
                            // Renew 1 minute before expiration or if it was invalidated
                            await TryGetOrAddCrlAsync(new SerialNumber(issuer.SerialNumber),
                                TimeSpan.FromMinutes(1), ct).ConfigureAwait(false);
                        }
                        if (issuers.ContinuationToken == null) {
                            break;
                        }
                        issuers = await _certificates.ListCertificatesAsync(
                            issuers.ContinuationToken, null, ct).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Exception occurred during crl cache management");
                }
                await Task.Delay(TimeSpan.FromMinutes(2), ct).ConfigureAwait(false);
            }
        }

        private readonly ILogger _logger;
        private readonly IDocumentCollection _crls;
        private readonly ICrlFactory _factory;
        private readonly ICertificateStore _certificates;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cts;
        private Task _cacheManager;
    }
}

