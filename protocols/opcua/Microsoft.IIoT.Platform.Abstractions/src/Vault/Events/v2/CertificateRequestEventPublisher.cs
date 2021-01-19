// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Events.v2 {
    using Microsoft.IIoT.Platform.Vault.Events.v2.Models;
    using Microsoft.IIoT.Platform.Vault.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// CertificateRequest registry event publisher
    /// </summary>
    public class CertificateRequestEventPublisher : ICertificateRequestListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public CertificateRequestEventPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnCertificateRequestSubmittedAsync(CertificateRequestModel request) {
            return _bus.PublishAsync(Wrap(CertificateRequestEventType.Submitted, request));
        }

        /// <inheritdoc/>
        public Task OnCertificateRequestApprovedAsync(CertificateRequestModel request) {
            return _bus.PublishAsync(Wrap(CertificateRequestEventType.Approved, request));
        }

        /// <inheritdoc/>
        public Task OnCertificateRequestAcceptedAsync(CertificateRequestModel request) {
            return _bus.PublishAsync(Wrap(CertificateRequestEventType.Accepted, request));
        }

        /// <inheritdoc/>
        public Task OnCertificateRequestCompletedAsync(CertificateRequestModel request) {
            return _bus.PublishAsync(Wrap(CertificateRequestEventType.Completed, request));
        }

        /// <inheritdoc/>
        public Task OnCertificateRequestDeletedAsync(CertificateRequestModel request) {
            return _bus.PublishAsync(Wrap(CertificateRequestEventType.Deleted, request));
        }


        /// <summary>
        /// Create request event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static CertificateRequestEventModel Wrap(CertificateRequestEventType type,
            CertificateRequestModel request) {
            return new CertificateRequestEventModel {
                EventType = type,
                Request = request
            };
        }

        private readonly IEventBusPublisher _bus;
    }
}
