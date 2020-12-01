// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Models {
    using Microsoft.IIoT.Crypto.Models;
    using System.Linq;
    using System;

    /// <summary>
    /// A X509 certificate revocation list extensions
    /// </summary>
    public static class X509CrlModelEx {

        /// <summary>
        /// Create crl
        /// </summary>
        /// <param name="crl"></param>
        public static X509CrlModel ToServiceModel(this Crl crl) {
            if (crl is null) {
                throw new ArgumentNullException(nameof(crl));
            }
            return new X509CrlModel {
                Crl = crl.RawData.ToArray(),
                Issuer = crl.Issuer
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <returns></returns>
        public static Crl ToStackModel(this X509CrlModel model) {
            return CrlEx.ToCrl(model.ToRawData());
        }

        /// <summary>
        /// Get Raw data
        /// </summary>
        /// <returns></returns>
        public static byte[] ToRawData(this X509CrlModel model) {
            if (model is null) {
                throw new ArgumentNullException(nameof(model));
            }

            const string certPemHeader = "-----BEGIN X509 CRL-----";
            const string certPemFooter = "-----END X509 CRL-----";
            if (model.Crl == null) {
                throw new ArgumentException("Crl data missing", nameof(model));
            }
            if (model.Crl.IsBytes) {
                return (byte[])model.Crl;
            }
            if (model.Crl.IsString) {
                var request = (string)model.Crl;
                if (request.Contains(certPemHeader,
                    StringComparison.OrdinalIgnoreCase)) {
                    var strippedCertificateRequest = request.Replace(
                        certPemHeader, "", StringComparison.OrdinalIgnoreCase);
                    strippedCertificateRequest = strippedCertificateRequest.Replace(
                        certPemFooter, "", StringComparison.OrdinalIgnoreCase);
                    return Convert.FromBase64String(strippedCertificateRequest);
                }
                return Convert.FromBase64String(request);
            }
            throw new ArgumentException("Bad crl data.", nameof(model));
        }
    }
}
