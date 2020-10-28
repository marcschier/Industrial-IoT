// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Security.Cryptography.X509Certificates {
    using Microsoft.Azure.IIoT.Crypto;
    using Microsoft.Azure.IIoT.Crypto.BouncyCastle;
    using Microsoft.Azure.IIoT.Crypto.Models;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Certificate request
    /// </summary>
    public static class CertificateRequestEx {

        /// <summary>
        /// Create request with public key
        /// </summary>
        /// <param name="pubKey"></param>
        /// <param name="subjectDN"></param>
        /// <param name="signatureType"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static CertificateRequest CreateCertificateRequest(this Key pubKey,
            X500DistinguishedName subjectDN, SignatureType signatureType,
            IEnumerable<X509Extension> extensions = null) {
            var request = new CertificateRequest(subjectDN, pubKey.ToPublicKey(),
                signatureType.ToHashAlgorithmName());
            if (extensions != null) {
                foreach (var extension in extensions) {
                    request.CertificateExtensions.Add(extension);
                }
            }
            return request;
        }

        /// <summary>
        /// Convert buffer to certificate request
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="signatureType"></param>
        /// <returns></returns>
        public static CertificateRequest ToCertificateRequest(
            this IReadOnlyCollection<byte> buffer,
            SignatureType signatureType = SignatureType.RS256) {
            var csr = buffer.ToCertificationRequestInfo();
            var key = csr.GetPublicKey();
            var extensions = new List<X509Extension>();
            foreach (var extension in csr.GetX509Extensions().ToX509Extensions()) {
                extensions.Add(extension);
            }
            return key.CreateCertificateRequest(
                new X500DistinguishedName(csr.Subject.GetEncoded()),
                signatureType, extensions);
        }

        /// <summary>
        /// Get public key from request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Key GetPublicKey(this CertificateRequest request) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }

            var csr = request.CreateSigningRequest().ToCertificationRequestInfo();
            return csr.GetPublicKey();
        }

        /// <summary>
        /// Create signed certificate from request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="signer"></param>
        /// <param name="issuer"></param>
        /// <param name="notBefore"></param>
        /// <param name="notAfter"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public static X509Certificate2 Create(this CertificateRequest request, IDigestSigner signer,
            Certificate issuer, DateTime notBefore, DateTime notAfter,
            IReadOnlyCollection<byte> serialNumber) {
            if (issuer is null) {
                throw new ArgumentNullException(nameof(issuer));
            }
            return Create(request, signer, issuer.Subject, issuer.KeyHandle,
                issuer.IssuerPolicies.SignatureType.Value, notBefore, notAfter, serialNumber);
        }

        /// <summary>
        /// Create signed certificate from request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="signer"></param>
        /// <param name="issuer"></param>
        /// <param name="signingKey"></param>
        /// <param name="signatureType"></param>
        /// <param name="notBefore"></param>
        /// <param name="notAfter"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public static X509Certificate2 Create(this CertificateRequest request, IDigestSigner signer,
            X500DistinguishedName issuer, KeyHandle signingKey, SignatureType signatureType,
            DateTime notBefore, DateTime notAfter, IReadOnlyCollection<byte> serialNumber) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }
            var signatureGenerator = signer.CreateX509SignatureGenerator(
                signingKey, signatureType);
            var signedCert = request.Create(issuer, signatureGenerator, notBefore,
                notAfter, serialNumber.ToArray());
            return signedCert;
        }
    }
}
