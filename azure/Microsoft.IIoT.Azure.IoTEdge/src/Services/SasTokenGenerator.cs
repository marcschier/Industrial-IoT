﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Services {
    using Microsoft.IIoT.Azure.IoTEdge;
    using Microsoft.IIoT.Extensions.Crypto;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Generates a sas token
    /// </summary>
    public class SasTokenGenerator : ISasTokenGenerator, IIoTEdgeDigestSigner {

        /// <summary>
        /// Edge hub connection string
        /// </summary>
        internal ConnectionString EdgeHubConnectionString {
            get {
                var cs = _options.CurrentValue.EdgeHubConnectionString;
                if (!_hsm.IsPresent && !string.IsNullOrEmpty(cs) &&
                    ConnectionString.TryParse(cs, out var o)) {
                    return o;
                }
                return null;
            }
        }

        /// <summary>
        /// Create a sas token generator
        /// </summary>
        /// <param name="options"></param>
        /// <param name="hsm"></param>
        /// <param name="cache"></param>
        /// <param name="identity"></param>
        public SasTokenGenerator(IOptionsMonitor<IoTEdgeClientOptions> options,
            IIdentity identity, ISecureElement hsm, ICache cache) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _hsm = hsm ?? throw new ArgumentNullException(nameof(hsm));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc/>
        public async Task<string> GenerateTokenAsync(string audience,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(audience)) {
                audience = _identity.Hub;
            }
            else {
                audience = FormatAudience(audience);
            }
            if (string.IsNullOrEmpty(audience)) {
                throw new ArgumentNullException(nameof(audience));
            }

            var cs = EdgeHubConnectionString;
            var keyId = cs?.SharedAccessKeyName;
            if (_hsm.IsPresent || string.IsNullOrEmpty(keyId)) {
                keyId = "primary";
            }
            var cacheKey = keyId + ":" + audience;
            var rawToken = await _cache.GetStringAsync(cacheKey, ct).ConfigureAwait(false);
            if (SasToken.IsValid(rawToken)) {
                return rawToken;
            }

            // If not found or not valid, create new token with configured lifetime...
            var lifetime = _options.CurrentValue.TokenLifetime ?? kDefaultTokenLifetime;
            var expiration = DateTime.UtcNow + lifetime;
            var token = await SasToken.CreateAsync(audience, expiration,
                async (kid, value, ct) => {
                    var toSign = Encoding.UTF8.GetBytes(value);
                    var signature = await SignAsync(kid, toSign, cs,
                        ct).ConfigureAwait(false);
                    return Convert.ToBase64String(signature);
                },
                _identity.DeviceId, _identity.ModuleId, keyId, ct).ConfigureAwait(false);
            rawToken = token.ToString();
            await _cache.SetStringAsync(audience + keyId, rawToken,
                DateTime.UtcNow + (lifetime * 0.75), ct).ConfigureAwait(false);
            return rawToken;
        }

        /// <inheritdoc/>
        public Task<byte[]> SignAsync(byte[] payload,
            CancellationToken ct) {
            var cs = EdgeHubConnectionString;
            var keyId = cs?.SharedAccessKeyName;
            if (_hsm.IsPresent || string.IsNullOrEmpty(keyId)) {
                keyId = "primary";
            }
            return SignAsync(keyId, payload, EdgeHubConnectionString, ct);
        }

        /// <summary>
        /// Format audience by stripping off query parameters if any.
        /// </summary>
        /// <param name="audience"></param>
        /// <returns></returns>
        private static string FormatAudience(string audience) {
            return audience.Split('?')[0];
        }

        /// <summary>
        /// Create signature
        /// </summary>
        /// <param name="toSign"></param>
        /// <param name="keyId"></param>
        /// <param name="cs"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<byte[]> SignAsync(string keyId, byte[] toSign,
            ConnectionString cs, CancellationToken ct) {
            if (_hsm.IsPresent) {
                return await _hsm.SignAsync(toSign, keyId, ct: ct).ConfigureAwait(false);
            }
            if (string.IsNullOrEmpty(cs?.SharedAccessKey)) {
                throw new ArgumentException("No key material present to sign token.");
            }
            else {
                var key = Convert.FromBase64String(cs.SharedAccessKey);
                using (var algorithm = new HMACSHA256(key)) {
                    return algorithm.ComputeHash(toSign);
                }
            }
        }

        private static readonly TimeSpan kDefaultTokenLifetime = TimeSpan.FromMinutes(10);
        private readonly ISecureElement _hsm;
        private readonly ICache _cache;
        private readonly IOptionsMonitor<IoTEdgeClientOptions> _options;
        private readonly IIdentity _identity;
    }
}