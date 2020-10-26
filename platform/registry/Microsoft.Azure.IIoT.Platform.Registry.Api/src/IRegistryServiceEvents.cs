// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Directory service events
    /// </summary>
    public interface IRegistryServiceEvents {

        /// <summary>
        /// Subscribe to gateway events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeGatewayEventsAsync(
            Func<GatewayEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to supervisor events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeSupervisorEventsAsync(
            Func<SupervisorEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to discoverer events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDiscovererEventsAsync(
            Func<DiscovererEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to publisher events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribePublisherEventsAsync(
            Func<PublisherEventApiModel, Task> callback);
    }
}
