﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Supervisor registry change listener
    /// </summary>
    public class SupervisorEventBusSubscriber : IEventHandler<SupervisorEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public SupervisorEventBusSubscriber(IEnumerable<ISupervisorRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<ISupervisorRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(SupervisorEventModel eventData) {
            switch (eventData.EventType) {
                case SupervisorEventType.New:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnSupervisorNewAsync(
                            eventData.Context, eventData.Supervisor)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case SupervisorEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnSupervisorUpdatedAsync(
                            eventData.Context, eventData.Supervisor)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case SupervisorEventType.Deleted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnSupervisorDeletedAsync(
                            eventData.Context, eventData.Id)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<ISupervisorRegistryListener> _listeners;
    }
}