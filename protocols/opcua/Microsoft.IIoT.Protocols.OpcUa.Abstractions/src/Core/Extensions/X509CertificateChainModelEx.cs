// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Models {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Certificate Chain extensions
    /// </summary>
    public static class X509CertificateChainModelEx {

        /// <summary>
        /// Convert raw buffer to certificate chain
        /// </summary>
        /// <param name="rawCertificates"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        public static X509CertificateChainModel ToCertificateChain(
            this IReadOnlyCollection<byte> rawCertificates, bool validate = true) {
            if (rawCertificates == null) {
                return null;
            }
            var certificates = new List<X509Certificate2>();
            try {
                while (true) {
                    var cur = new X509Certificate2(rawCertificates.ToArray());
                    certificates.Add(cur);
                    if (cur.RawData.Length >= rawCertificates.Count) {
                        break;
                    }
                    rawCertificates = rawCertificates
                        .Skip(cur.RawData.Length)
                        .ToArray();
                }
                return new X509CertificateChainModel {
                    Status = validate ? certificates.Validate() : null,
                    Chain = certificates
                        .Select(c => c.ToServiceModel())
                        .ToList()
                };
            }
            finally {
                certificates.ForEach(c => c.Dispose());
            }
        }

        /// <summary>
        /// Gets the leaf thumprint
        /// </summary>
        /// <param name="rawCertificates"></param>
        /// <returns></returns>
        public static string ToThumbprint(this IReadOnlyCollection<byte> rawCertificates) {
            try {
                var chain = rawCertificates.ToCertificateChain(false)?.Chain;
                if (chain == null || chain.Count == 0) {
                    return null;
                }
                return chain[chain.Count - 1].Thumbprint;
            }
            catch {
                // Fall back to sha256 which was the previous thumprint algorithm
                return rawCertificates.ToArray().ToSha256Hash();
            }
        }

        /// <summary>
        /// Validate certificate chain
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static List<X509ChainStatus> Validate(this IEnumerable<X509Certificate2> chain) {
            using (var validator = new X509Chain(false)) {
                validator.ChainPolicy.RevocationFlag =
                    X509RevocationFlag.EntireChain;
                validator.ChainPolicy.RevocationMode =
                    X509RevocationMode.NoCheck;
                validator.ChainPolicy.ExtraStore.AddRange(
                     new X509Certificate2Collection(chain.SkipLast(1).ToArray()));
                validator.Build(chain.Last());
                var result = new List<X509ChainStatus>();
                foreach (var item in validator.ChainElements) {
                    var state = X509ChainStatusFlags.NoError;
                    foreach (var status in item.ChainElementStatus) {
                        state |= status.Status;
                    }
                    result.Add(state.ToServiceModel());
                }
                return result;
            }
        }
    }
}
