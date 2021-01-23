// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Api {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Registry service events
    /// </summary>
    public interface IDiscoveryServiceEvents {

        /// <summary>
        /// Subscribe to application events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeApplicationEventsAsync(
            Func<ApplicationEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to endpoint events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeEndpointEventsAsync(
            Func<EndpointEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to supervisor discovery events
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDiscoveryProgressByDiscovererIdAsync(
            string discovererId, Func<DiscoveryProgressApiModel, Task> callback);

        /// <summary>
        /// Subscribe to discovery events for a particular request
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDiscoveryProgressByRequestIdAsync(
            string requestId, Func<DiscoveryProgressApiModel, Task> callback);
    }
}
