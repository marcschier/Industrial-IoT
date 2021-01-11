﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Opc.Ua {
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.IIoT.Extensions.Utils;

    /// <summary>
    /// Certificate store extensions
    /// </summary>
    public static class CertificateStoreEx {

        /// <summary>
        /// Add to certificate store
        /// </summary>
        /// <param name="store"></param>
        /// <param name="certificates"></param>
        /// <param name="noCopy"></param>
        /// <returns></returns>
        public static void Add(this ICertificateStore store,
            IEnumerable<X509Certificate2> certificates,
            bool noCopy = false) {
            if (store is null) {
                throw new ArgumentNullException(nameof(store));
            }
            if (certificates == null) {
                throw new ArgumentNullException(nameof(certificates));
            }
            foreach (var cert in certificates) {
                Try.Op(() => store.Delete(cert.Thumbprint));
                if (noCopy) {
                    store.Add(cert);
                }
                else {
                    using var copy = new X509Certificate2(cert);
                    store.Add(copy);
                }
            }
        }

        /// <summary>
        /// Remove from certificate store
        /// </summary>
        /// <param name="store"></param>
        /// <param name="certificates"></param>
        /// <returns></returns>
        public static void Remove(this ICertificateStore store,
            IEnumerable<X509Certificate2> certificates) {
            if (store is null) {
                throw new ArgumentNullException(nameof(store));
            }
            if (certificates == null) {
                throw new ArgumentNullException(nameof(certificates));
            }
            foreach (var cert in certificates) {
                store.Delete(cert.Thumbprint);
            }
        }

        /// <summary>
        /// </summary>
        /// Try remove from trust store
        /// <param name="store"></param>
        /// <param name="certificates"></param>
        /// <returns></returns>
        public static bool TryRemove(this ICertificateStore store,
            IEnumerable<X509Certificate2> certificates) {
            try {
                Remove(store, certificates);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
