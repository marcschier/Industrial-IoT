// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Runtime {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Runtime;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Azure.IoTEdge.Runtime;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Wraps a configuration root
    /// </summary>
    public class Config : ConfigBase, IIoTEdgeConfig, IClientServicesConfig,
        IDiagnosticsConfig, IMetricServerConfig {

        /// <inheritdoc/>
        public string EdgeHubConnectionString => _ie.EdgeHubConnectionString;
        /// <inheritdoc/>
        public bool BypassCertVerification => _ie.BypassCertVerification;
        /// <inheritdoc/>
        public TransportOption Transport => _ie.Transport;

        /// <inheritdoc/>
        public string ApplicationName => _opc.ApplicationName;
        /// <inheritdoc/>
        public string ApplicationUri => _opc.ApplicationUri;
        /// <inheritdoc/>
        public string ProductUri => _opc.ProductUri;
        /// <inheritdoc/>
        public uint DefaultSessionTimeout => _opc.DefaultSessionTimeout;
        /// <inheritdoc/>
        public int KeepAliveInterval => _opc.KeepAliveInterval;
        /// <inheritdoc/>
        public uint MaxKeepAliveCount => _opc.MaxKeepAliveCount;
        /// <inheritdoc/>
        public int MinSubscriptionLifetime => _opc.MinSubscriptionLifetime;
        /// <inheritdoc/>
        public string PkiRootPath => _opc.PkiRootPath;
        /// <inheritdoc/>
        public CertificateInfo ApplicationCertificate => _opc.ApplicationCertificate;
        /// <inheritdoc/>
        public bool AutoAcceptUntrustedCertificates => _opc.AutoAcceptUntrustedCertificates;
        /// <inheritdoc/>
        public ushort MinimumCertificateKeySize => _opc.MinimumCertificateKeySize;
        /// <inheritdoc/>
        public CertificateStore RejectedCertificateStore => _opc.RejectedCertificateStore;
        /// <inheritdoc/>
        public bool RejectSha1SignedCertificates => _opc.RejectSha1SignedCertificates;
        /// <inheritdoc/>
        public bool AddAppCertToTrustedStore => _opc.AddAppCertToTrustedStore;
        /// <inheritdoc/>
        public CertificateStore TrustedIssuerCertificates => _opc.TrustedIssuerCertificates;
        /// <inheritdoc/>
        public CertificateStore TrustedPeerCertificates => _opc.TrustedPeerCertificates;
        /// <inheritdoc/>
        public int ChannelLifetime => _opc.ChannelLifetime;
        /// <inheritdoc/>
        public int MaxArrayLength => _opc.MaxArrayLength;
        /// <inheritdoc/>
        public int MaxBufferSize => _opc.MaxBufferSize;
        /// <inheritdoc/>
        public int MaxByteStringLength => _opc.MaxByteStringLength;
        /// <inheritdoc/>
        public int MaxMessageSize => _opc.MaxMessageSize;
        /// <inheritdoc/>
        public int MaxStringLength => _opc.MaxStringLength;
        /// <inheritdoc/>
        public int OperationTimeout => _opc.OperationTimeout;
        /// <inheritdoc/>
        public int SecurityTokenLifetime => _opc.SecurityTokenLifetime;

        /// <inheritdoc/>
        public DiagnosticsLevel DiagnosticsLevel => _ms.DiagnosticsLevel;
        /// <inheritdoc/>
        public TimeSpan? MetricsCollectionInterval => _ms.MetricsCollectionInterval;
        /// <inheritdoc/>
        public int Port => _ms.Port;
        /// <inheritdoc/>
        public string Path => _ms.Path;
        /// <inheritdoc/>
        public bool UseHttps => _ms.UseHttps;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration) :
            base(configuration) {
            _ie = new IoTEdgeConfig(configuration);
            _opc = new ClientServicesConfig(configuration);
            _ms = new MetricsServerConfig(configuration);
        }

        private readonly MetricsServerConfig _ms;
        private readonly ClientServicesConfig _opc;
        private readonly IoTEdgeConfig _ie;
    }
}
