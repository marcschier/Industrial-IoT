// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Configuration {
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Net;

    /// <summary>
    /// Configuration base helper class
    /// </summary>
    public abstract class ConfigureOptionBase {

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        protected ConfigureOptionBase(IConfiguration configuration) {
            if (configuration == null) {
                configuration = new ConfigurationBuilder()
                    .Build();
            }
            Configuration = configuration;
        }

        /// <summary>
        /// Read variable and replace environment variable if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected string GetStringOrDefault(string key, string defaultValue = null) {
            var value = Configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(value)) {
                return defaultValue;
            }
            return value.Trim();
        }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected bool GetBoolOrDefault(string key, bool defaultValue = false) {
            var result = GetBoolOrNull(key);
            if (result != null) {
                return result.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected bool? GetBoolOrNull(string key, bool? defaultValue = null) {
            var value = GetStringOrDefault(key, string.Empty).ToLowerInvariant();
            var knownTrue = new HashSet<string> { "true", "yes", "y", "1" };
            var knownFalse = new HashSet<string> { "false", "no", "n", "0" };
            if (knownTrue.Contains(value)) {
                return true;
            }
            if (knownFalse.Contains(value)) {
                return false;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get time span
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected TimeSpan GetDurationOrDefault(string key,
            TimeSpan defaultValue = default) {
            var result = GetDurationOrNull(key);
            if (result == null) {
                return defaultValue;
            }
            return result.Value;
        }

        /// <summary>
        /// Get time span
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected TimeSpan? GetDurationOrNull(string key,
            TimeSpan? defaultValue = null) {
            if (!TimeSpan.TryParse(GetStringOrDefault(key), out var result)) {
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// Read int
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected int GetIntOrDefault(string key, int defaultValue = 0) {
            var value = GetIntOrNull(key);
            if (value.HasValue) {
                return value.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Read int
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected int? GetIntOrNull(string key, int? defaultValue = null) {
            try {
                var value = GetStringOrDefault(key, null);
                if (string.IsNullOrEmpty(value)) {
                    return defaultValue;
                }
                return Convert.ToInt32(value);
            }
            catch {
                return defaultValue;
            }
        }

        /// <summary>
        /// Read variable and get connection string token from it
        /// </summary>
        /// <param name="key"></param>
        /// <param name="getter"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected string GetConnectonStringTokenOrDefault(string key,
            Func<ConnectionString, string> getter, string defaultValue = null) {
            var value = Configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(value)
                || !ConnectionString.TryParse(value.Trim(), out var cs)
                || string.IsNullOrEmpty(value = getter(cs))) {
                if (defaultValue == null) {
                    return string.Empty;
                }
                return defaultValue;
            }
            return value;
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
