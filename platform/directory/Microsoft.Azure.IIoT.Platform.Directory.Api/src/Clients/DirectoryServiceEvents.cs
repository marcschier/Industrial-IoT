// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api {
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Events.Api;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Directory service event client
    /// </summary>
    public class DirectoryServiceEvents : IDirectoryServiceEvents {

        /// <summary>
        /// Event client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="client"></param>
        public DirectoryServiceEvents(ICallbackClient client, IEventsConfig config) :
            this(client, config?.OpcUaEventsServiceUrl) {
        }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceUri"></param>
        public DirectoryServiceEvents(ICallbackClient client, string serviceUri) {
            if (string.IsNullOrWhiteSpace(serviceUri)) {
                throw new ArgumentNullException(nameof(serviceUri),
                    "Please configure the Url of the events micro service.");
            }
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serviceUri = serviceUri.TrimEnd('/');
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeGatewayEventsAsync(
            Func<GatewayEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/gateways/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.GatewayEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeSupervisorEventsAsync(
            Func<SupervisorEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/supervisors/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.SupervisorEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDiscovererEventsAsync(
            Func<DiscovererEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/discovery/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DiscovererEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribePublisherEventsAsync(
            Func<PublisherEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/publishers/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.PublisherEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        private readonly string _serviceUri;
        private readonly ICallbackClient _client;
    }
}
