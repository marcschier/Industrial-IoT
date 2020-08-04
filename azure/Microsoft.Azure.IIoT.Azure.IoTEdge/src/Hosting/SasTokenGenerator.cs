// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Cryptography;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Auth;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Generates a sas token
    /// </summary>
    public class SasTokenGenerator : ISasTokenGenerator {

        /// <summary>
        /// Create a sas token generator
        /// </summary>
        /// <param name="config"></param>
        /// <param name="hsm"></param>
        /// <param name="cache"></param>
        /// <param name="identity"></param>
        public SasTokenGenerator(IIoTEdgeConfig config, IIdentity identity, ISecureElement hsm,
            ICache cache) {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _hsm = hsm ?? throw new ArgumentNullException(nameof(hsm));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (!hsm.IsPresent && !string.IsNullOrEmpty(config?.EdgeHubConnectionString)) {
                _cs = ConnectionString.Parse(config.EdgeHubConnectionString);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateTokenAsync(string audience, CancellationToken ct) {
            if (string.IsNullOrEmpty(audience)) {
                throw new ArgumentNullException(nameof(audience));
            }
            var keyId = _cs?.SharedAccessKeyName;
            if (_hsm.IsPresent || string.IsNullOrEmpty(keyId)) {
                keyId = "primary";
            }
            audience = FormatAudience(audience);
            var cacheKey = keyId + ":" + audience;
            var rawToken = await _cache.GetStringAsync(cacheKey, ct);
            if (SasToken.IsValid(rawToken)) {
                return rawToken;
            }
            // If not found or not valid, create new token with default lifetime...
            var expiration = DateTime.UtcNow + kDefaultTokenLifetime;
            var token = await SasToken.CreateAsync(audience, expiration,
                SignTokenAsync, _identity.DeviceId, _identity.ModuleId, keyId);
            rawToken = token.ToString();
            await _cache.SetStringAsync(audience + keyId, rawToken,
                expiration - kTokenCacheRenewal, ct);
            return rawToken;
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
        /// <param name="value"></param>
        /// <param name="keyId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<string> SignTokenAsync(string keyId, string value,
            CancellationToken ct) {
            var toSign = Encoding.UTF8.GetBytes(value);
            byte[] signature;
            if (_hsm.IsPresent) {
                signature = await _hsm.SignAsync(toSign, keyId);
            }
            else if (string.IsNullOrEmpty(_cs?.SharedAccessKey)) {
                throw new ArgumentException("No key material present to sign token.");
            }
            else {
                var key = Convert.FromBase64String(_cs.SharedAccessKey);
                using (var algorithm = new HMACSHA256(key)) {
                    signature = algorithm.ComputeHash(toSign);
                }
            }
            return Convert.ToBase64String(signature);
        }

        private static readonly TimeSpan kDefaultTokenLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan kTokenCacheRenewal = TimeSpan.FromMinutes(5);
        private readonly ISecureElement _hsm;
        private readonly ICache _cache;
        private readonly IIdentity _identity;
        private readonly ConnectionString _cs;
    }
}
