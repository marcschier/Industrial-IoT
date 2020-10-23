// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Clients {
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of configuration services using service sdk.
    /// </summary>
    public sealed class IoTHubConfigurationClient : IDeviceDeploymentServices {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public IoTHubConfigurationClient(IIoTHubConfig config, ILogger logger) {
            if (string.IsNullOrEmpty(config?.IoTHubConnString)) {
                throw new ArgumentException("Missing connection string", nameof(config));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registry = RegistryManager.CreateFromConnectionString(config.IoTHubConnString);
            _registry.OpenAsync().Wait();
        }


        /// <inheritdoc/>
        public async Task ApplyConfigurationAsync(string deviceId,
            ConfigurationContentModel configuration, CancellationToken ct) {
            try {
                await _registry.ApplyConfigurationContentOnDeviceAsync(deviceId,
                    configuration.ToContent(), ct).ConfigureAwait(false);
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Apply configuration failed ");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<ConfigurationModel> CreateOrUpdateConfigurationAsync(
            ConfigurationModel configuration, bool forceUpdate, CancellationToken ct) {

            try {
                if (string.IsNullOrEmpty(configuration.Etag)) {
                    // First try create configuration
                    try {
                        var added = await _registry.AddConfigurationAsync(
                            configuration.ToConfiguration(), ct).ConfigureAwait(false);
                        return added.ToModel();
                    }
                    catch (DeviceAlreadyExistsException) when (forceUpdate) {
                        //
                        // Technically update below should now work but for
                        // some reason it does not.
                        // Remove and re-add in case we are forcing updates.
                        //
                        await _registry.RemoveConfigurationAsync(configuration.Id, ct).ConfigureAwait(false);
                        var added = await _registry.AddConfigurationAsync(
                            configuration.ToConfiguration(), ct).ConfigureAwait(false);
                        return added.ToModel();
                    }
                }

                // Try update existing configuration
                var result = await _registry.UpdateConfigurationAsync(
                    configuration.ToConfiguration(), forceUpdate, ct).ConfigureAwait(false);
                return result.ToModel();
            }
            catch (Exception e) {
                _logger.LogTrace(e,
                    "Update configuration failed in CreateOrUpdate");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<ConfigurationModel> GetConfigurationAsync(
            string configurationId, CancellationToken ct) {
            try {
                var configuration = await _registry.GetConfigurationAsync(
                    configurationId, ct).ConfigureAwait(false);
                return configuration.ToModel();
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Get configuration failed");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ConfigurationModel>> ListConfigurationsAsync(
            int? maxCount, CancellationToken ct) {
            try {
                var configurations = await _registry.GetConfigurationsAsync(
                    maxCount ?? int.MaxValue, ct).ConfigureAwait(false);
                return configurations.Select(c => c.ToModel());
            }
            catch (Exception e) {
                _logger.LogTrace(e, "List configurations failed");
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteConfigurationAsync(string configurationId,
            string etag, CancellationToken ct) {
            try {
                if (string.IsNullOrEmpty(etag)) {
                    await _registry.RemoveConfigurationAsync(configurationId, ct).ConfigureAwait(false);
                }
                else {
                    await _registry.RemoveConfigurationAsync(
                        new Configuration(configurationId) { ETag = etag }, ct).ConfigureAwait(false);
                }
            }
            catch (Exception e) {
                _logger.LogTrace(e, "Delete configuration failed");
                throw e.Translate();
            }
        }

        private readonly RegistryManager _registry;
        private readonly ILogger _logger;
    }
}
