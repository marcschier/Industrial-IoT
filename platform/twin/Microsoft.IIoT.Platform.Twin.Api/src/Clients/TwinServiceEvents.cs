// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api {
    using Microsoft.IIoT.Platform.Twin.Api.Models;
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Rpc;
    using Microsoft.IIoT.Http;
    using Microsoft.IIoT.Utils;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin service event client
    /// </summary>
    public class TwinServiceEvents : ITwinServiceEvents {

        /// <summary>
        /// Event client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="client"></param>
        public TwinServiceEvents(ICallbackClient client, IEventsConfig config) :
            this(client, config?.OpcUaEventsServiceUrl) {
        }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceUri"></param>
        public TwinServiceEvents(ICallbackClient client, string serviceUri) {
            if (string.IsNullOrWhiteSpace(serviceUri)) {
                throw new ArgumentNullException(nameof(serviceUri),
                    "Please configure the Url of the events micro service.");
            }
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serviceUri = serviceUri.TrimEnd('/');
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeTwinEventsAsync(
            Func<TwinEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/twins/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.TwinEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        private readonly string _serviceUri;
        private readonly ICallbackClient _client;
    }
}
