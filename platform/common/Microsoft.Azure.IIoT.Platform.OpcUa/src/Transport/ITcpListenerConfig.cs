// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Transport {
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Tcp listener configuration
    /// </summary>
    public interface ITcpListenerConfig {

        /// <summary>
        /// Certificate to use
        /// </summary>
        X509Certificate2 TcpListenerCertificate { get; }

        /// <summary>
        /// Chain
        /// </summary>
        X509Certificate2Collection TcpListenerCertificateChain { get; }

        /// <summary>
        /// Certificate validator
        /// </summary>
        X509CertificateValidator CertificateValidator { get; }

        /// <summary>
        /// Public dns address
        /// </summary>
        string PublicDnsAddress { get; }

        /// <summary>
        /// Port to listen on for tcp support (default: 4840)
        /// </summary>
        int Port { get; }
    }
}
