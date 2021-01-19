﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;
    using Opc.Ua.Client;

    /// <summary>
    /// Session manager
    /// </summary>
    public interface ISessionManager {

        /// <summary>
        /// Number of sessions
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// Get or create session for subscription
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        Session GetOrCreateSession(ConnectionModel connection, bool createIfNotExists);

        /// <summary>
        /// Remove session if empty
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="onlyIfEmpty"></param>
        /// <returns></returns>
        Task RemoveSessionAsync(ConnectionModel connection, bool onlyIfEmpty = true);

        /// <summary>
        /// Get or create a subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        void RegisterSubscription(ISubscriptionHandle subscription);

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        void UnregisterSubscription(ISubscriptionHandle subscription);

        /// <summary>
        /// stops all pending sessions
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}