// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Handlers {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Supervisor event handler
    /// </summary>
    public sealed class SupervisorTwinEventHandler : IDeviceTwinEventHandler {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="broker"></param>
        /// <param name="logger"></param>
        public SupervisorTwinEventHandler(IDeviceTwinServices iothub,
            IRegistryEventBroker<ISupervisorRegistryListener> broker, ILogger logger) {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task HandleDeviceTwinEventAsync(DeviceTwinEvent ev) {
            if (ev.Handled) {
                return;
            }
            if (string.IsNullOrEmpty(ev.Twin.Id) || string.IsNullOrEmpty(ev.Twin.ModuleId)) {
                return;
            }
            var type = ev.Twin.Properties?.Reported.GetValueOrDefault<string>(
                TwinProperty.Type, null);
            if ((ev.Event != DeviceTwinEventType.Delete && ev.IsPatch) || string.IsNullOrEmpty(type)) {
                try {
                    ev.Twin = await _iothub.GetAsync(ev.Twin.Id, ev.Twin.ModuleId).ConfigureAwait(false);
                    ev.IsPatch = false;
                    type = ev.Twin.Properties?.Reported?.GetValueOrDefault<string>(
                        TwinProperty.Type, null);
                }
                catch (Exception ex) {
                    _logger.LogTrace(ex, "Failed to materialize twin");
                }
            }
            if (IdentityType.Supervisor.EqualsIgnoreCase(type)) {
                var ctx = new OperationContextModel {
                    AuthorityId = ev.AuthorityId,
                    Time = ev.Timestamp
                };
                switch (ev.Event) {
                    case DeviceTwinEventType.New:
                        break;
                    case DeviceTwinEventType.Create:
                        await _broker.NotifyAllAsync(l => l.OnSupervisorNewAsync(ctx,
                            ev.Twin.ToSupervisorRegistration(false).ToServiceModel())).ConfigureAwait(false);
                        break;
                    case DeviceTwinEventType.Update:
                        await _broker.NotifyAllAsync(l => l.OnSupervisorUpdatedAsync(ctx,
                            ev.Twin.ToSupervisorRegistration(false).ToServiceModel())).ConfigureAwait(false);
                        break;
                    case DeviceTwinEventType.Delete:
                        await _broker.NotifyAllAsync(l => l.OnSupervisorDeletedAsync(ctx,
                            HubResource.Format(ev.Twin.Hub, ev.Twin.Id, ev.Twin.ModuleId))).ConfigureAwait(false);
                        break;
                }
                ev.Handled = true;
            }
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IRegistryEventBroker<ISupervisorRegistryListener> _broker;
        private readonly ILogger _logger;
    }
}
