// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Gateway.Runtime {
    using Microsoft.IIoT.Protocols.OpcUa.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Transport;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Common web service configuration aggregation
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<WebHostOptions>,
        ITcpListenerConfig, IWebListenerConfig,
        ISessionServicesConfig, IDiscoveryConfig {

        /// <inheritdoc/>
        public string[] ListenUrls => null;
        /// <inheritdoc/>
        public X509Certificate2 TcpListenerCertificate => null;
        /// <inheritdoc/>
        public X509Certificate2Collection TcpListenerCertificateChain => null;
        /// <inheritdoc/>
        public X509CertificateValidator CertificateValidator => null;
        /// <inheritdoc/>
        public string PublicDnsAddress => null;
        /// <inheritdoc/>
        public int Port => 51111; //  Utils.UaTcpDefaultPort;
        /// <inheritdoc/>
        public TimeSpan MaxRequestAge => _sessions.MaxRequestAge;
        /// <inheritdoc/>
        public int NonceLength => _sessions.NonceLength;
        /// <inheritdoc/>
        public int MaxSessionCount => _sessions.MaxSessionCount;
        /// <inheritdoc/>
        public TimeSpan MaxSessionTimeout => _sessions.MaxSessionTimeout;
        /// <inheritdoc/>
        public TimeSpan MinSessionTimeout => _sessions.MinSessionTimeout;

        /// <inheritdoc/>
        public string DiscoveryServiceUrl => _api.DiscoveryServiceUrl;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {

            _sessions = new SessionServicesConfig(configuration);
            _api = new DiscoveryConfig(configuration);
        }

        /// <inheritdoc/>
        public override void Configure(string name, WebHostOptions options) {
            options.ServicePathBase = GetStringOrDefault(
                PcsVariable.PCS_OPCUA_GATEWAY_SERVICE_PATH_BASE);
        }

        private readonly SessionServicesConfig _sessions;
        private readonly DiscoveryConfig _api;
    }
}
