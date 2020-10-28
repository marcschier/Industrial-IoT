// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans.Grains {
    using Microsoft.Azure.IIoT.Services.Orleans.Clients;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using global::Orleans;

    /// <summary>
    /// Orleans event bus as grain
    /// </summary>
    public class OrleansEventBusGrain : Grain, IEventBus {

        /// <summary>
        /// Create event bus client
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        /// 
        public OrleansEventBusGrain(IJsonSerializer serializer, ILogger logger) {
            _bus = new OrleansEventBusClient(new OrleansGrainClient(GrainFactory),
                serializer, logger);
        }

        /// <inheritdoc/>
        public Task PublishAsync<T>(T message) {
            return _bus.PublishAsync(message);
        }

        /// <inheritdoc/>
        public Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            return _bus.RegisterAsync(handler);
        }

        /// <inheritdoc/>
        public Task UnregisterAsync(string token) {
            return _bus.UnregisterAsync(token);
        }

        private readonly OrleansEventBusClient _bus;
    }
}