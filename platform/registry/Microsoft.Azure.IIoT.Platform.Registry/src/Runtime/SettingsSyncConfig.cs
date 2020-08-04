// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Runtime {
    using Microsoft.Azure.IIoT.Platform.Edge;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Default edge endpoint configuration
    /// </summary>
    public class SettingsSyncConfig : ConfigBase, IServiceEndpoint, ISettingsSyncConfig {

        /// <summary>
        /// Property keys
        /// </summary>
        private const string kServiceEndpointUrlKey = "ServiceEndpointUrl";
        private const string kSettingSyncIntervalKey = "SettingSyncInterval";

        /// <inheritdoc/>
        public TimeSpan? SettingSyncInterval => GetDurationOrNull(kSettingSyncIntervalKey);

        /// <inheritdoc/>
        public string ServiceEndpoint => GetStringOrDefault(kServiceEndpointUrlKey,
            () => GetStringOrDefault(PcsVariable.PCS_EDGE_SERVICE_URL,
                () => GetDefaultUrl("9051", "edge")));
        /// <inheritdoc/>
        public event EventHandler OnServiceEndpointUpdated;

        /// <summary>
        /// Create endpoint config
        /// </summary>
        /// <param name="configuration"></param>
        public SettingsSyncConfig(IConfiguration configuration) :
            base(configuration) {
            OnServiceEndpointUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Get endpoint url
        /// </summary>
        /// <param name="port"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GetDefaultUrl(string port, string path) {
            var cloudEndpoint = GetStringOrDefault(PcsVariable.PCS_SERVICE_URL)?.Trim()?.TrimEnd('/');
            if (string.IsNullOrEmpty(cloudEndpoint)) {
                // Test port is open
                if (!int.TryParse(port, out var nPort)) {
                    return $"http://localhost:9080/{path}";
                }
                using (var socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Unspecified)) {
                    try {
                        socket.Connect(IPAddress.Loopback, nPort);
                        return $"http://localhost:{port}";
                    }
                    catch {
                        return $"http://localhost:9080/{path}";
                    }
                }
            }
            return $"{cloudEndpoint}/{path}";
        }
    }
}
