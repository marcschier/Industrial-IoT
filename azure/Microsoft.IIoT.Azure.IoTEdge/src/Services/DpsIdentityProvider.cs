// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Services {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Azure.Devices.Provisioning.Client;
    using Microsoft.Azure.Devices.Provisioning.Client.Transport;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Security.Cryptography;

    /// <summary>
    /// Identity provider service can enroll a child device using the parent device
    /// identity with a cloud dps enrollment. Requires an enrollment group created for
    /// the edge identity represented as <see cref="IIdentity"/>.
    /// </summary>
    public sealed class DpsIdentityProvider : IIoTEdgeIdentityProvider, IDisposable {

        /// <summary>
        /// Create provisioning client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="identity"></param>
        /// <param name="signer"></param>
        /// <param name="logger"></param>
        public DpsIdentityProvider(IIdentity identity, IIoTEdgeDigestSigner signer,
            IOptions<ProvisioningClientOptions> options, ILogger logger) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transport = new ProvisioningTransportHandlerMqtt();
            _rand = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public async Task<string> GetConnectionStringAsync(string deviceId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var desiredKey = new byte[64];
            _rand.GetNonZeroBytes(desiredKey);
            var deviceKey = await _signer.SignAsync(desiredKey, ct).ConfigureAwait(false);

            using var provider = new SecurityProviderSymmetricKey(deviceId,
                Convert.ToBase64String(deviceKey), null);
            var client = ProvisioningDeviceClient.Create(_options.Value.Endpoint,
                _options.Value.IdScope, provider, _transport);

            var result = await client.RegisterAsync(ct).ConfigureAwait(false);
            if (result.Status != ProvisioningRegistrationStatusType.Assigned) {
                _logger.LogError("Failed to register device {deviceId}. " +
                    "Status was {status} with {error}: {message}", deviceId,
                    result.Status, result.ErrorCode, result.ErrorMessage);
                throw new ResourceInvalidStateException(
                    $"{result.ErrorCode}: {result.ErrorMessage}");
            }

            var authentication = new DeviceAuthenticationWithRegistrySymmetricKey(
                result.DeviceId, provider.GetPrimaryKey());
            return IotHubConnectionStringBuilder.Create(result.AssignedHub,
                _identity.Gateway, authentication).ToString();
        }

        /// <inheritdoc/>
        public void Dispose() {
            _transport.Dispose();
            _rand.Dispose();
        }

        private readonly ProvisioningTransportHandler _transport;
        private readonly IIoTEdgeDigestSigner _signer;
        private readonly IOptions<ProvisioningClientOptions> _options;
        private readonly IIdentity _identity;
        private readonly RandomNumberGenerator _rand;
        private readonly ILogger _logger;
    }
}
