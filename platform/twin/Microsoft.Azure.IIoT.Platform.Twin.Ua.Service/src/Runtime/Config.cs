// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Ua.Service.Runtime {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Runtime;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Transport;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Cors;
    using Microsoft.Azure.IIoT.AspNetCore.Cors.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.AppInsights.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Common web service configuration aggregation
    /// </summary>
    public class Config : ConfigBase, IWebHostConfig,
        ICorsConfig, ITcpListenerConfig, IWebListenerConfig,
        ISessionServicesConfig, IDiscoveryConfig, IHeadersConfig {


        /// <inheritdoc/>
        public string CorsWhitelist => _cors.CorsWhitelist;
        /// <inheritdoc/>
        public bool CorsEnabled => _cors.CorsEnabled;

        /// <inheritdoc/>
        public int HttpsRedirectPort => _host.HttpsRedirectPort;
        /// <inheritdoc/>
        public string ServicePathBase => GetStringOrDefault(
            PcsVariable.PCS_GATEWAY_SERVICE_PATH_BASE,
            () => _host.ServicePathBase);

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
        public string OpcUaRegistryServiceUrl => _api.OpcUaRegistryServiceUrl;

        /// <inheritdoc/>
        public bool AspNetCoreForwardedHeadersEnabled =>
            _fh.AspNetCoreForwardedHeadersEnabled;
        /// <inheritdoc/>
        public int AspNetCoreForwardedHeadersForwardLimit =>
            _fh.AspNetCoreForwardedHeadersForwardLimit;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration) :
            base(configuration) {

            _host = new WebHostConfig(configuration);
            _cors = new CorsConfig(configuration);
            _sessions = new SessionServicesConfig(configuration);
            _api = new DiscoveryConfig(configuration);
            _fh = new HeadersConfig(configuration);
        }

        private readonly WebHostConfig _host;
        private readonly CorsConfig _cors;
        private readonly SessionServicesConfig _sessions;
        private readonly DiscoveryConfig _api;
        private readonly HeadersConfig _fh;
    }
}
