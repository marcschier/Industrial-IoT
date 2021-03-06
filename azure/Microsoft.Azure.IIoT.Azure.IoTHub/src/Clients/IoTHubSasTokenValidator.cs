﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Clients {
    using Microsoft.Azure.IIoT.Auth;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Sas token validator
    /// </summary>
    public class IoTHubSasTokenValidator : ISasTokenValidator {

        /// <summary>
        /// Create validator
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="cache"></param>
        public IoTHubSasTokenValidator(IDeviceTwinServices hub, ICache cache) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        /// <inheritdoc/>
        public async Task<string> ValidateToken(string sharedAccessToken) {
            if (string.IsNullOrEmpty(sharedAccessToken)) {
                throw new UnauthorizedAccessException();
            }
            var token = SasToken.Parse(sharedAccessToken);
            _ = token.ParseIdentities(out var deviceId, out var moduleId);
            if (string.IsNullOrEmpty(deviceId)) {
                throw new UnauthorizedAccessException();
            }
            var key = await _cache.GetStringAsync(token.Audience);
            if (key == null || !token.Authenticate(key)) {
                var registration = await _hub.GetRegistrationAsync(
                    deviceId, moduleId);
                key = registration.Authentication.PrimaryKey;
                if (key == null || !token.Authenticate(key)) {
                    // Try secondary key
                    key = registration.Authentication.SecondaryKey;
                    if (key == null || !token.Authenticate(key)) {
                        // Failed to validate
                        throw new UnauthorizedAccessException();
                    }
                }
                await _cache.SetStringAsync(token.Audience, key,
                    DateTime.UtcNow + TimeSpan.FromHours(1));
            }
            return $"_{deviceId}_{moduleId ?? ""}";
        }

        private readonly ICache _cache;
        private readonly IDeviceTwinServices _hub;
    }
}