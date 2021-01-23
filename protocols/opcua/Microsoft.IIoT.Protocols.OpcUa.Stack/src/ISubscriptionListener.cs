// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;
    using Opc.Ua.Client;
    using Opc.Ua;

    /// <summary>
    /// Subscription listener
    /// </summary>
    /// <returns></returns>
    public interface ISubscriptionListener {

        /// <summary>
        /// Called when connectivity changes
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="newState"></param>
        /// 
        void OnConnectivityChange(ConnectionStatus previous,
            ConnectionStatus newState);

        /// <summary>
        /// Monitored item state changed
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="subscription"></param>
        /// <param name="monitoredItemId"></param>
        /// <param name="isEvent"></param>
        /// <param name="clientHandle"></param>
        /// <param name="serverId"></param>
        /// <param name="lastResult"></param>
        /// 
        void OnMonitoredItemStatusChange(string subscriptionId,
            Subscription subscription, string monitoredItemId,
            bool isEvent, uint? clientHandle, uint? serverId,
            ServiceResult lastResult = null);

        /// <summary>
        /// Subscription changed
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="subscription"></param>
        /// <param name="lastResult"></param>
        /// 
        void OnSubscriptionStatusChange(string subscriptionId,
            Subscription subscription, ServiceResult lastResult = null);

        /// <summary>
        /// Called for data notifications
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="subscription"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// 
        void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, DataChangeNotification notification,
            IList<string> stringTable);

        /// <summary>
        /// Called for event notifications
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="subscription"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// 
        void OnSubscriptionNotification(string subscriptionId,
            Subscription subscription, EventNotificationList notification,
            IList<string> stringTable);
    }
}