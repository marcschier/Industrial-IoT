// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Handlers {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Registry;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Gateway event handler.
    /// </summary>
    public sealed class GatewayTwinEventHandler : IDeviceTwinEventHandler {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="broker"></param>
        /// <param name="logger"></param>
        public GatewayTwinEventHandler(IDeviceTwinServices iothub,
            IRegistryEventBroker<IGatewayRegistryListener> broker, ILogger logger) {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task HandleDeviceTwinEventAsync(DeviceTwinEvent ev) {
            if (ev.Handled) {
                return;
            }
            if (string.IsNullOrEmpty(ev.Twin.Id)) {
                return;
            }
            var type = ev.Twin.Tags?.GetValueOrDefault<string>(TwinProperty.Type, null);
            if ((ev.Event != DeviceTwinEventType.Delete && ev.IsPatch) || string.IsNullOrEmpty(type)) {
                try {
                    ev.Twin = await _iothub.GetAsync(ev.Twin.Id).ConfigureAwait(false);
                    ev.IsPatch = false;
                    type = ev.Twin.Tags?.GetValueOrDefault<string>(TwinProperty.Type, null);
                }
                catch (Exception ex) {
                    _logger.LogTrace(ex, "Failed to materialize twin");
                }
            }
            if (IdentityType.Gateway.EqualsIgnoreCase(type)) {
                var ctx = new OperationContextModel {
                    AuthorityId = ev.AuthorityId,
                    Time = ev.Timestamp
                };
                switch (ev.Event) {
                    case DeviceTwinEventType.New:
                        break;
                    case DeviceTwinEventType.Create:
                        await _broker.NotifyAllAsync(l => l.OnGatewayNewAsync(ctx,
                            ev.Twin.ToGatewayRegistration().ToServiceModel())).ConfigureAwait(false);
                        break;
                    case DeviceTwinEventType.Update:
                        await _broker.NotifyAllAsync(l => l.OnGatewayUpdatedAsync(ctx,
                            ev.Twin.ToGatewayRegistration().ToServiceModel())).ConfigureAwait(false);
                        break;
                    case DeviceTwinEventType.Delete:
                        await _broker.NotifyAllAsync(l => l.OnGatewayDeletedAsync(ctx,
                            ev.Twin.Id)).ConfigureAwait(false);
                        break;
                }
                ev.Handled = true;
            }
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IRegistryEventBroker<IGatewayRegistryListener> _broker;
        private readonly ILogger _logger;
    }
}
