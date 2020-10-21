// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Opc.Ua.Client;

    /// <summary>
    /// Subscription abstraction
    /// </summary>
    public interface ISubscriptionHandle : IDisposable {

        /// <summary>
        /// Identifier of the subscription
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Enabled - successfully created on server
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Publishing is active
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Connection
        /// </summary>
        ConnectionModel Connection { get; }

        /// <summary>
        /// Update connectivity status
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="newValue"></param>
        void UpdateConnectivityState(ConnectionStatus previous,
            ConnectionStatus newValue);

        /// <summary>
        /// Apply desired state
        /// </summary>
        /// <param name="monitoredItems"></param>
        /// <param name="configuration"></param>
        /// <returns>enabled</returns>
        Task ApplyAsync(IEnumerable<MonitoredItemModel> monitoredItems,
            SubscriptionConfigurationModel configuration);

        /// <summary>
        /// Creates the subscription and it's associated monitored items
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task EnableAsync(Session session);

        /// <summary>
        /// Sets the subscription and it's monitored items in publishing respective
        /// reporting state
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task ActivateAsync(Session session);

        /// <summary>
        /// disables publishing for the subscription
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task DeactivateAsync(Session session);

        /// <summary>
        /// Close and delete subscription
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}