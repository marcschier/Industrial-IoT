// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a shared access token
    /// </summary>
    public class SasToken {

        /// <summary>
        /// Expires
        /// </summary>
        public DateTime ExpiresOn { get; }

        /// <summary>
        /// Audience
        /// </summary>
        public string Audience { get; }

        /// <summary>
        /// Name of the key used
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Signature
        /// </summary>
        public string Signature { get; }

        /// <summary>
        /// Create sas token
        /// </summary>
        /// <param name="audience"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="expiresOn"></param>
        /// <param name="signatureFactory"></param>
        /// <param name="keyName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<SasToken> CreateAsync(string audience, DateTime expiresOn,
            Func<string, string, CancellationToken, Task<string>> signatureFactory,
            string deviceId = null, string moduleId = null, string keyName = null,
            CancellationToken ct = default) {
            if (!string.IsNullOrEmpty(deviceId)) {
                audience += $"/devices/{WebUtility.UrlEncode(deviceId)}";
            }
            if (!string.IsNullOrEmpty(moduleId)) {
                audience += $"/modules/{WebUtility.UrlEncode(moduleId)}";
            }
            var encodedAudience = WebUtility.UrlEncode(audience);

            var secondsFromBaseTime = expiresOn.Subtract(kEpochTime);
            var seconds = Convert.ToInt64(secondsFromBaseTime.TotalSeconds,
                CultureInfo.InvariantCulture);
            var expiry = Convert.ToString(seconds, CultureInfo.InvariantCulture);

            var signature = await signatureFactory(keyName, encodedAudience + "\n" + expiry, ct);
            return new SasToken(signature, expiresOn, expiry, keyName, encodedAudience);
        }

        /// <summary>
        /// Parse token
        /// </summary>
        /// <param name="sharedAccessToken"></param>
        /// <returns></returns>
        public static SasToken Parse(string sharedAccessToken) {
            if (string.IsNullOrWhiteSpace(sharedAccessToken)) {
                throw new ArgumentNullException(nameof(sharedAccessToken));
            }

            var parsedFields = ExtractFieldValues(sharedAccessToken);
            if (!parsedFields.TryGetValue(kSignatureFieldName, out var signature)) {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture,
                    "Missing field: {0}", kSignatureFieldName));
            }
            if (!parsedFields.TryGetValue(kExpiryFieldName, out var expiry)) {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture,
                    "Missing field: {0}", kExpiryFieldName));
            }
            if (!parsedFields.TryGetValue(kAudienceFieldName, out var encodedAudience)) {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture,
                    "Missing field: {0}", kAudienceFieldName));
            }

            // KeyName (skn) is optional.
            parsedFields.TryGetValue(kKeyNameFieldName, out var keyName);

            var expiresOn = kEpochTime +
                TimeSpan.FromSeconds(double.Parse(expiry, CultureInfo.InvariantCulture));
            return new SasToken(signature, expiresOn, expiry, keyName, encodedAudience);
        }

        /// <summary>
        /// Validate the raw token
        /// </summary>
        /// <param name="sharedAccessToken"></param>
        /// <param name="clockSkew"></param>
        /// <returns></returns>
        public static bool IsValid(string sharedAccessToken, TimeSpan? clockSkew = null) {
            if (string.IsNullOrWhiteSpace(sharedAccessToken)) {
                return false;
            }
            try {
                var parsedFields = ExtractFieldValues(sharedAccessToken);
                if (parsedFields == null) {
                    return false;
                }
                if (!parsedFields.TryGetValue(kSignatureFieldName, out var signature)) {
                    return false;
                }
                if (!parsedFields.TryGetValue(kExpiryFieldName, out var expiry)) {
                    return false;
                }
                if (!parsedFields.TryGetValue(kAudienceFieldName, out var encodedAudience)) {
                    return false;
                }
                var expiresOn = kEpochTime +
                    TimeSpan.FromSeconds(double.Parse(expiry, CultureInfo.InvariantCulture));
                if (expiresOn + (clockSkew ?? TimeSpan.Zero) < DateTime.UtcNow) {
                    return false;
                }
                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Create token
        /// </summary>
        /// <param name="expiresOn"></param>
        /// <param name="expiry"></param>
        /// <param name="keyName"></param>
        /// <param name="signature"></param>
        /// <param name="encodedAudience"></param>
        private SasToken(string signature, DateTime expiresOn, string expiry, string keyName,
            string encodedAudience) {
            _encodedAudience = encodedAudience;
            _expiry = expiry;
            ExpiresOn = expiresOn;
            Signature = signature;
            Audience = WebUtility.UrlDecode(encodedAudience);
            KeyName = keyName ?? string.Empty;
        }

        /// <inheritdoc/>
        public override string ToString() {
            var buffer = new StringBuilder();
            buffer.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}={2}&{3}={4}&{5}={6}",
                kSharedAccessSignature,
                kAudienceFieldName, _encodedAudience,
                kSignatureFieldName, WebUtility.UrlEncode(Signature),
                kExpiryFieldName, WebUtility.UrlEncode(_expiry));
            return buffer.ToString();
        }

        /// <summary>
        /// Authenticate token against a base64 encoded key
        /// </summary>
        /// <param name="base64EncodedKey"></param>
        /// <param name="clockSkew"></param>
        public bool Authenticate(string base64EncodedKey, TimeSpan? clockSkew = null) {
            if (ExpiresOn + (clockSkew ?? TimeSpan.FromMinutes(5)) < DateTime.UtcNow) {
                return false;
            }
            using (var algorithm = new HMACSHA256(Convert.FromBase64String(base64EncodedKey))) {
                var computed = Convert.ToBase64String(algorithm.ComputeHash(
                    Encoding.UTF8.GetBytes(_encodedAudience + "\n" + _expiry)));
                if (StringComparer.Ordinal.Equals(Signature, computed)) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Authorize target
        /// </summary>
        /// <param name="targetAddress"></param>
        public void Authorize(Uri targetAddress) {
            if (targetAddress == null) {
                throw new ArgumentNullException(nameof(targetAddress));
            }
            var target = targetAddress.Host + targetAddress.AbsolutePath;
            if (!target.StartsWith(Audience.TrimEnd(new char[] { '/' }),
                StringComparison.OrdinalIgnoreCase)) {
                throw new UnauthorizedAccessException("Invalid target audience");
            }
        }

        /// <summary>
        /// Parse identities
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public string ParseIdentities(out string deviceId, out string moduleId) {
            var elements = Audience.Split("/devices/", StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length > 1) {
                var elements2 = elements[elements.Length - 1].Split("/modules/",
                    StringSplitOptions.RemoveEmptyEntries);
                deviceId = elements2[0];
                if (elements.Length == 2) {
                    moduleId = elements2[1];
                }
                else {
                    moduleId = null;
                }
            }
            else {
                deviceId = null;
                moduleId = null;
            }
            return elements[0];
        }

        /// <summary>
        /// Parse fields
        /// </summary>
        /// <param name="rawToken"></param>
        /// <returns></returns>
        private static IDictionary<string, string> ExtractFieldValues(string rawToken) {
            var lines = rawToken.Split();
            if (!string.Equals(lines[0].Trim(), kSharedAccessSignature, StringComparison.Ordinal) ||
                lines.Length != 2) {
                throw new FormatException("Malformed signature");
            }
            var parsedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var fields = lines[1].Trim()
                .Split(new string[] { kPairSeparator }, StringSplitOptions.None);
            foreach (var field in fields) {
                if (string.IsNullOrEmpty(field)) {
                    continue;
                }
                var fieldParts = field.Split(
                    new string[] { kKeyValueSeparator }, StringSplitOptions.None);
                if (fieldParts[0].EqualsIgnoreCase(kAudienceFieldName)) {
                    // defer decoding the URL until later.
                    parsedFields.Add(fieldParts[0], fieldParts[1]);
                }
                else {
                    parsedFields.Add(fieldParts[0], WebUtility.UrlDecode(fieldParts[1]));
                }
            }
            return parsedFields;
        }

        private const string kSharedAccessSignature = "SharedAccessSignature";
        private const string kAudienceFieldName = "sr";
        private const string kSignatureFieldName = "sig";
        private const string kKeyNameFieldName = "skn";
        private const string kExpiryFieldName = "se";
        private const string kKeyValueSeparator = "=";
        private const string kPairSeparator = "&";
        private static readonly DateTime kEpochTime =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private readonly string _encodedAudience;
        private readonly string _expiry;
    }
}





