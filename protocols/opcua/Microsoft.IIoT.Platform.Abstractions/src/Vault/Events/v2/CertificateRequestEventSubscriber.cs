// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Events.v2 {
    using Microsoft.IIoT.Platform.Vault.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Certificate Request change listener
    /// </summary>
    public class CertificateRequestEventSubscriber : IEventBusConsumer<CertificateRequestEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public CertificateRequestEventSubscriber(IEnumerable<ICertificateRequestListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<ICertificateRequestListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(CertificateRequestEventModel eventData) {
            switch (eventData.EventType) {
                case CertificateRequestEventType.Submitted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnCertificateRequestSubmittedAsync(
                            eventData.Request)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case CertificateRequestEventType.Approved:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnCertificateRequestApprovedAsync(
                            eventData.Request)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case CertificateRequestEventType.Completed:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnCertificateRequestCompletedAsync(
                            eventData.Request)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case CertificateRequestEventType.Accepted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnCertificateRequestAcceptedAsync(
                            eventData.Request)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case CertificateRequestEventType.Deleted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnCertificateRequestDeletedAsync(
                            eventData.Request)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<ICertificateRequestListener> _listeners;
    }
}
