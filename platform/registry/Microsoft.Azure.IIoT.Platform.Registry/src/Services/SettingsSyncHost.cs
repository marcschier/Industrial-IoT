// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Edge;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Update settings on all module entities
    /// </summary>
    public class SettingsSyncHost : AbstractRunHost {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="twins"></param>
        /// <param name="endpoint"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public SettingsSyncHost(IDeviceTwinServices twins, IServiceEndpoint endpoint,
            IJsonSerializer serializer, ILogger logger, ISettingsSyncConfig config = null) :
            base(logger, "Service Endpoint Update",
                config?.SettingSyncInterval ?? TimeSpan.FromMinutes(1)) {

            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _twins = twins ?? throw new ArgumentNullException(nameof(twins));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _endpoint.OnServiceEndpointUpdated += OnServiceEndpointUpdated;
        }

        /// <inheritdoc/>
        protected override void OnDisposing() {
            _endpoint.OnServiceEndpointUpdated -= OnServiceEndpointUpdated;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken ct) {
            var url = _endpoint.ServiceEndpoint?.TrimEnd('/');
            if (string.IsNullOrEmpty(url)) {
                return;
            }
            var query = "SELECT * FROM devices.modules WHERE " +
                $"IS_DEFINED(properties.reported.{TwinProperty.ServiceEndpoint}) AND " +
                $"(NOT IS_DEFINED(properties.desired.{TwinProperty.ServiceEndpoint}) OR " +
                    $"properties.desired.{TwinProperty.ServiceEndpoint} != '{url}')";
            string continuation = null;
            do {
                var response = await _twins.QueryDeviceTwinsAsync(
                    query, continuation, null, ct);
                foreach (var moduleTwin in response.Items) {
                    try {
                        moduleTwin.Properties.Desired[TwinProperty.ServiceEndpoint] =
                            _serializer.FromObject(url);
                        await _twins.PatchAsync(moduleTwin, false, ct);
                    }
                    catch (Exception ex) {
                        _logger.Error(ex, "Failed to update url for module {device} {module}",
                            moduleTwin.Id, moduleTwin.ModuleId);
                    }
                }
                continuation = response.ContinuationToken;
                ct.ThrowIfCancellationRequested();
            }
            while (continuation != null);
            _logger.Information("Endpoint url updated to {url} on all module twins.", url);
        }

        /// <summary>
        /// Service endpoint was updated - sync now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceEndpointUpdated(object sender, EventArgs e) {
            RunNow();
        }

        private readonly IServiceEndpoint _endpoint;
        private readonly IJsonSerializer _serializer;
        private readonly IDeviceTwinServices _twins;
        private readonly ILogger _logger;
    }
}