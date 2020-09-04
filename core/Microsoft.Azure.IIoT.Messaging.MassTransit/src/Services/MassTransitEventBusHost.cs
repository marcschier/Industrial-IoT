// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit.Services {
    using Microsoft.Azure.IIoT.Messaging.Default;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using global::MassTransit;

    /// <summary>
    /// Event bus host that also controls the underlying bus
    /// </summary>
    public class MassTransitEventBusHost : IHostProcess, IDisposable {

        /// <summary>
        /// Create mass transit bus
        /// </summary>
        /// <param name="client"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        /// <param name="bus"></param>
        public MassTransitEventBusHost(IBusControl bus, IEventBus client,
            IEnumerable<IHandler> handlers, ILogger logger) {
            _host = new EventBusHost(client, handlers, logger);
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _bus.StartAsync();
            try {
                await _host.StartAsync();
            }
            catch {
                await _bus.StopAsync();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            try {
                await _host.StopAsync();
            }
            finally {
                await _bus.StopAsync();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _host.Dispose();
        }

        private readonly EventBusHost _host;
        private readonly IBusControl _bus;
    }
}