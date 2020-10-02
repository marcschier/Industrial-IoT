// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Crypto.Storage {
    using Microsoft.Azure.IIoT.Crypto.Models;
    using Microsoft.Azure.IIoT.Crypto.Storage.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Certificate store using an underlying database.
    /// </summary>
    public class CertificateDatabase : ICertificateRepository, ICertificateStore {

        /// <summary>
        /// Create database
        /// </summary>
        /// <param name="container"></param>
        /// <param name="keys"></param>
        public CertificateDatabase(IItemContainerFactory container,
            IKeyHandleSerializer keys) {
            if (container is null) {
                throw new ArgumentNullException(nameof(container));
            }
            _certificates = container.OpenAsync("certificates").Result;
            _keys = keys ?? throw new ArgumentNullException(nameof(keys));
        }

        /// <inheritdoc/>
        public async Task AddCertificateAsync(string certificateName,
            Certificate certificate, string id, CancellationToken ct) {
            var document = certificate.ToDocument(certificateName, id, _keys);
            _ = await _certificates.UpsertAsync(document, ct: ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Certificate> FindCertificateAsync(string certificateId,
            CancellationToken ct) {
            var result = _certificates.CreateQuery<CertificateDocument>(1)
                .Where(x => x.Type == nameof(Certificate))
                .Where(x => x.CertificateId == certificateId)
                .OrderByDescending(x => x.Version)
                .Take(1)
                .GetResults();
            var documents = await result.ReadAsync(ct).ConfigureAwait(false);
            return DocumentToCertificate(documents.SingleOrDefault()?.Value);
        }

        /// <inheritdoc/>
        public async Task<string> DisableCertificateAsync(Certificate certificate,
            CancellationToken ct) {
            if (certificate?.RawData == null) {
                throw new ArgumentNullException(nameof(certificate));
            }
            var now = DateTime.UtcNow;
            while (true) {
                var document = await _certificates.FindAsync<CertificateDocument>(
                    certificate.GetSerialNumberAsString(), ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Certificate was not found");
                }
                if (document.Value?.DisabledSince != null) {
                    // Already disabled
                    return document.Value.CertificateId;
                }
                try {
                    var newDocument = document.Value.Clone();
                    newDocument.DisabledSince = now;
                    document = await _certificates.ReplaceAsync(document,
                        newDocument, ct: ct).ConfigureAwait(false);

                    // TODO: Notify disabled certificate
                    return document.Value.CertificateId;
                }
                catch (ResourceOutOfDateException) {
                    continue; // Replace failed due to etag out of date - retry
                }
            }
        }

        /// <inheritdoc/>
        public async Task<Certificate> GetCertificateAsync(
            IReadOnlyCollection<byte> serialNumber, CancellationToken ct) {
            var serial = new SerialNumber(serialNumber).ToString();
            var document = await _certificates.GetAsync<CertificateDocument>(serial,
                ct: ct).ConfigureAwait(false);
            return DocumentToCertificate(document?.Value);
        }

        /// <inheritdoc/>
        public async Task<Certificate> FindLatestCertificateAsync(string certificateName,
            CancellationToken ct) {
            var result = _certificates.CreateQuery<CertificateDocument>(1)
                .Where(x => x.Type == nameof(Certificate))
                .Where(x => x.CertificateName == certificateName)
                .OrderByDescending(x => x.Version)
                .Take(1)
                .GetResults();
            var documents = await result.ReadAsync(ct).ConfigureAwait(false);
            return DocumentToCertificate(documents.SingleOrDefault()?.Value);
        }

        /// <inheritdoc/>
        public async Task<CertificateCollection> QueryCertificatesAsync(
            CertificateFilter filter, int? pageSize, CancellationToken ct) {

            var query = _certificates.CreateQuery<CertificateDocument>(pageSize)
                .Where(x => x.Type == nameof(Certificate));
            if (filter != null) {
                if (filter.NotBefore != null) {
                    query = query.Where(x => x.NotBefore <= filter.NotBefore.Value);
                }
                if (filter.NotAfter != null) {
                    query = query.Where(x => x.NotAfter >= filter.NotAfter.Value);
                }
                if (filter.IncludeDisabled && filter.ExcludeEnabled) {
                    query = query.Where(x => x.DisabledSince != null);
                }
                if (!filter.IncludeDisabled && !filter.ExcludeEnabled) {
                    query = query.Where(x => x.DisabledSince == null);
                }
                if (filter.CertificateName != null) {
                    query = query.Where(x => x.CertificateName == filter.CertificateName);
                }
                if (filter.Subject != null) {
                    var subject = filter.Subject.Name;
                    if (filter.IncludeAltNames) {
                        query = query.Where(x => x.Subject == subject ||
                            (x.SubjectAltNames != null && x.SubjectAltNames.Contains(subject)));
                    }
                    else {
                        query = query.Where(x => x.Subject == subject);
                    }
                }
                if (filter.Thumbprint != null) {
                    query = query.Where(x => x.Thumbprint == filter.Thumbprint);
                }
                if (filter.KeyId != null) {
                    query = query.Where(x => x.KeyId == filter.KeyId);
                }
                if (filter.IsIssuer != null) {
                    query = query.Where(x => x.IsIssuer == filter.IsIssuer.Value);
                }
                if (filter.Issuer != null) {
                    var issuer = filter.Issuer.Name;
                    if (filter.IncludeAltNames) {
                        query = query.Where(x => x.Issuer == issuer ||
                            (x.IssuerAltNames != null && x.IssuerAltNames.Contains(issuer)));
                    }
                    else {
                        query = query.Where(x => x.Issuer == issuer);
                    }
                }
                if (filter.IssuerSerialNumber != null) {
                    var sn = new SerialNumber(filter.IssuerSerialNumber).ToString();
                    query = query.Where(x => x.IssuerSerialNumber == sn);
                }
                if (filter.IssuerKeyId != null) {
                    query = query.Where(x => x.IssuerKeyId == filter.IssuerKeyId);
                }
            }
            query = query.OrderByDescending(x => x.Version);
            var result = query.GetResults();
            var documents = await result.ReadAsync(ct).ConfigureAwait(false);
            return new CertificateCollection {
                Certificates = documents
                    .Select(c => DocumentToCertificate(c.Value))
                    .ToList(),
                ContinuationToken = result.ContinuationToken
            };
        }

        /// <inheritdoc/>
        public async Task<CertificateCollection> ListCertificateChainAsync(
            Certificate certificate, CancellationToken ct) {
            if (certificate?.RawData == null) {
                throw new ArgumentNullException(nameof(certificate));
            }
            var chain = await ListChainAsync(certificate, ct).ConfigureAwait(false);
            return new CertificateCollection {
                Certificates = chain.ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<CertificateCollection> ListCertificatesAsync(
            string continuationToken, int? pageSize, CancellationToken ct) {

            IResultFeed<IDocumentInfo<CertificateDocument>> result;
            if (!string.IsNullOrEmpty(continuationToken)) {
                result = _certificates.ContinueQuery<CertificateDocument>(
                    continuationToken, pageSize);
            }
            else {
                result = _certificates.CreateQuery<CertificateDocument>(pageSize)
                    .Where(x => x.Type == nameof(Certificate))
                    .OrderByDescending(x => x.Version)
                    .GetResults();
            }
            var documents = await result.ReadAsync(ct).ConfigureAwait(false);
            return new CertificateCollection {
                Certificates = documents
                    .Select(c => DocumentToCertificate(c.Value))
                    .ToList(),
                ContinuationToken = result.ContinuationToken
            };
        }

        /// <summary>
        /// Quick query by subject dn
        /// </summary>
        /// <param name="subjectName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Certificate> GetCertificateBySubjectAsync(X500DistinguishedName subjectName,
            CancellationToken ct) {
            var result = _certificates.CreateQuery<CertificateDocument>(1)
                .Where(x => x.Type == nameof(Certificate))
                // With matching name
                .Where(x => x.Subject == subjectName.Name)
                // Latest on top
                .OrderByDescending(x => x.Version)
                // Select top 1
                .Take(1)
                .GetResults();
            var documents = await result.ReadAsync(ct).ConfigureAwait(false);
            return DocumentToCertificate(documents.SingleOrDefault()?.Value);
        }

        /// <summary>
        /// Try list chain by serial number and if fails use subject names
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Certificate>> ListChainAsync(Certificate certificate,
            CancellationToken ct) {
            try {
                // Try find chain using serial and issuer serial
                return await GetChainBySerialAsync(certificate, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                // Try traditional x500 name matching
                return await GetChainByNameAsync(certificate, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get chain by serial number
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Certificate>> GetChainBySerialAsync(
            Certificate certificate, CancellationToken ct) {
            var chain = new List<Certificate> { certificate };
            // Compare subject and issuer serial number
            var issuer = certificate.GetIssuerSerialNumberAsString();
            if (string.IsNullOrEmpty(issuer)) {
                throw new ResourceNotFoundException("Issuer serial not found");
            }
            while (!certificate.IsSelfSigned()) {
                certificate = await GetCertificateAsync(certificate.IssuerSerialNumber, ct).ConfigureAwait(false);
                if (certificate?.RawData == null) {
                    throw new ResourceNotFoundException("Incomplete chain");
                }
                chain.Add(certificate);
            }
            // Reverse to have root first
            chain.Reverse();
            return Validate(chain);
        }

        /// <summary>
        /// Get chain using subject names
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Certificate>> GetChainByNameAsync(
            Certificate certificate, CancellationToken ct) {
            var chain = new List<Certificate> { certificate };
            // Compare subject and issuer names
            while (!certificate.IsSelfSigned()) {
                certificate = await GetCertificateBySubjectAsync(certificate.Issuer,
                    ct).ConfigureAwait(false);
                if (certificate?.RawData == null) {
                    throw new ResourceNotFoundException("Incomplete chain");
                }
                chain.Add(certificate);
            }
            // Reverse to have root first
            chain.Reverse();
            return Validate(chain);
        }

        /// <summary>
        /// Validate the chain
        /// </summary>
        /// <param name="chain"></param>
        private static IEnumerable<Certificate> Validate(List<Certificate> chain) {
            if (!chain.First().IsValidChain(chain, out var status)) {
                throw new CryptographicException(status.AsString("Chain invalid:"));
            }
            return chain;
        }

        /// <summary>
        /// Convert to certificate model
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private Certificate DocumentToCertificate(CertificateDocument document) {
            if (document == null) {
                return null;
            }
            var keyHandle = _keys.DeserializeHandle(document.KeyHandle);
            return CertificateEx.Create(document.RawData,
                keyHandle,
                document.IsserPolicies,
                document.DisabledSince == null ? null : new RevocationInfo {
                    Date = document.DisabledSince,
                    // ...
                });
        }

        private readonly IItemContainer _certificates;
        private readonly IKeyHandleSerializer _keys;
    }
}

