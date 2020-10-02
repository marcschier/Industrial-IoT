// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using global::Azure.Messaging.EventHubs.Producer;
    using global::Azure.Messaging.EventHubs;
    using global::Azure.Core;

    /// <summary>
    /// Event hub namespace client
    /// </summary>
    public sealed class EventHubQueueClient : IEventQueueClient, IEventClient,
        IAsyncDisposable, IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="provider"></param>
        public EventHubQueueClient(IEventHubClientConfig config,
            ITokenProvider provider = null) {
            if (string.IsNullOrEmpty(config?.EventHubConnString)) {
                throw new ArgumentException(nameof(config));
            }
            if (provider != null && provider.Supports(Resource.EventHub)) {
                var cs = ConnectionString.Parse(config.EventHubConnString);
                var credential = new EventHubTokenProvider(provider);
                _client = new EventHubProducerClient(cs.Endpoint,
                    config.EventHubPath, credential);
            }
            else {
                _client = new EventHubProducerClient(config.EventHubConnString,
                    config.EventHubPath); // ok if path is null - then uses cs
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var ev = new EventData(payload);
            ev.Properties.Add(EventProperties.Target, target);
            if (properties != null) {
                foreach (var prop in properties) {
                    ev.Properties.Add(prop.Key, prop.Value);
                }
            }
            await _client.SendAsync(ev.YieldReturn(), GetPk(target, null, partitionKey), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Send<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            var t = SendAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait();
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] payload,
            string contentType, string eventSchema, string contentEncoding,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var ev = CreateEvent(payload, target, contentType, eventSchema, contentEncoding);
            await _client.SendAsync(ev.YieldReturn(), GetPk(target, eventSchema, null), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var pk = GetPk(target, eventSchema, null);
            var events = await _client.CreateBatchAsync(pk, ct);
            try {
                foreach (var ev in batch
                    .Select(b =>
                        CreateEvent(b, target, contentType, eventSchema, contentEncoding))) {
                    if (!events.TryAdd(ev)) {
                        if (events.SizeInBytes == 0) {
                            throw new MessageSizeLimitException(
                                $"Max size of event is {events.MaximumSizeInBytes}");
                        }
                        await _client.SendAsync(events, ct).ConfigureAwait(false);
                        events.Dispose();
                        events = await _client.CreateBatchAsync(pk, ct); // next batch
                        if (!events.TryAdd(ev)) {
                            throw new MessageSizeLimitException(
                                $"Max size of event is {events.MaximumSizeInBytes}");
                        }
                    }
                }
                if (events.SizeInBytes != 0) {
                    await _client.SendAsync(events, ct).ConfigureAwait(false);
                }
            }
            finally {
                events?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            var t = SendEventAsync(target, payload, contentType, eventSchema,
                    contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait();
        }

        /// <inheritdoc/>
        public void Dispose() {
            DisposeAsync().AsTask().Wait();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            await Try.Async(() => _client.CloseAsync()).ConfigureAwait(false);
            await _client.DisposeAsync();
        }

        /// <summary>
        /// Calculate partition
        /// </summary>
        /// <param name="target"></param>
        /// <param name="schema"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        private static CreateBatchOptions GetPk(string target, string schema,
            string partitionKey) {
            var key = string.IsNullOrEmpty(partitionKey) ? target : partitionKey;
            if (!string.IsNullOrEmpty(schema)) {
                key += schema;
            }
            return new CreateBatchOptions {
                PartitionKey = key.ToLowerInvariant()
            };
        }

        /// <summary>
        /// Helper to create event from buffer and content type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private EventData CreateEvent(byte[] data, string target,
            string contentType, string eventSchema, string contentEncoding) {
            var ev = new EventData(data);
            ev.Properties.Add(EventProperties.Target, target);
            if (!string.IsNullOrEmpty(contentEncoding)) {
                ev.Properties.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(contentType)) {
                ev.Properties.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                ev.Properties.Add(EventProperties.EventSchema, eventSchema);
            }
            return ev;
        }

        /// <inheritdoc/>
        private class EventHubTokenProvider : TokenCredential {

            /// <inheritdoc/>
            public EventHubTokenProvider(ITokenProvider provider) {
                _provider = provider;
            }

            /// <inheritdoc/>
            public override AccessToken GetToken(
                TokenRequestContext requestContext, CancellationToken ct) {
                return GetTokenAsync(requestContext, ct).Result;
            }

            /// <inheritdoc/>
            public override async ValueTask<AccessToken> GetTokenAsync(
                TokenRequestContext requestContext, CancellationToken ct) {
                var result = await _provider.GetTokenForAsync(Resource.EventHub).ConfigureAwait(false);
                if (result == null) {
                    return default;
                }
                return new AccessToken(result.RawToken, result.ExpiresOn);
            }

            private readonly ITokenProvider _provider;
        }

        private readonly EventHubProducerClient _client;
    }
}
