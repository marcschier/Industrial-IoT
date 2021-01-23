// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Handlers {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Clients;
    using Microsoft.IIoT.Extensions.Tasks;
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles discovery requests received from the <see cref="DiscoveryRequestEventClient"/>
    /// instance and pushes them to the supervisor using the discovery services.
    /// </summary>
    public sealed class DiscoveryRequestEventHandler : IEventBusConsumer<DiscoveryRequestModel> {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="discovery"></param>
        /// <param name="processor"></param>
        public DiscoveryRequestEventHandler(IDiscoveryServices discovery,
            ITaskProcessor processor) {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        /// <inheritdoc/>
        public Task HandleAsync(DiscoveryRequestModel request) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }
            _processor.TrySchedule(() => _discovery.DiscoverAsync(request, request.Context),
                () => Task.CompletedTask);
            return Task.CompletedTask;
        }

        private readonly IDiscoveryServices _discovery;
        private readonly ITaskProcessor _processor;
    }
}
