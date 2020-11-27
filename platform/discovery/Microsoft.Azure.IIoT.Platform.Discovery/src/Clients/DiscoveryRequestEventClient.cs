// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Clients {
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Onboarding client triggers registry onboarding in the jobs agent.
    /// </summary>
    public sealed class DiscoveryRequestEventClient : IDiscoveryServices {

        /// <summary>
        /// Create onboarding client
        /// </summary>
        /// <param name="events"></param>
        public DiscoveryRequestEventClient(IEventBusPublisher events) {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(ServerRegistrationRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.DiscoveryUrl == null) {
                throw new ArgumentException("Missing discovery uri", nameof(request));
            }
            if (string.IsNullOrEmpty(request.Id)) {
                request.Id = Guid.NewGuid().ToString();
            }
            await DiscoverAsync(new DiscoveryRequestModel {
                Configuration = new DiscoveryConfigModel {
                    DiscoveryUrls = new List<string> { request.DiscoveryUrl },
                },
                Id = request.Id
            }, context.Clone(), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DiscoverAsync(DiscoveryRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            // TODO: Publish context
            await _events.PublishAsync(request).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task CancelAsync(DiscoveryCancelModel request,
            OperationContextModel context, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            // TODO: Publish context
            await _events.PublishAsync(request).ConfigureAwait(false);
        }

        private readonly IEventBusPublisher _events;
    }
}
