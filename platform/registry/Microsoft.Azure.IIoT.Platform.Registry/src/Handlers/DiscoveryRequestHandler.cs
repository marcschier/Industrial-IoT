// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Handlers {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Clients;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles discovery requests received from the <see cref="OnboardingClient"/>
    /// instance and pushes them to the supervisor using the discovery services.
    /// </summary>
    public sealed class DiscoveryRequestHandler : IEventHandler<DiscoveryRequestModel> {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="discovery"></param>
        /// <param name="processor"></param>
        public DiscoveryRequestHandler(IDiscovererServices discovery,
            ITaskProcessor processor) {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        /// <inheritdoc/>
        public Task HandleAsync(DiscoveryRequestModel request) {
            _processor.TrySchedule(() => _discovery.DiscoverAsync(request),
                () => Task.CompletedTask);
            return Task.CompletedTask;
        }

        private readonly IDiscovererServices _discovery;
        private readonly ITaskProcessor _processor;
    }
}
