// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Handler {
    using Microsoft.Azure.IIoT.Platform.Vault.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle registry entity events and cleanup requests
    /// </summary>
    public sealed class RegistryEventHandler : IApplicationRegistryListener,
        IEndpointRegistryListener {

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="requests"></param>
        public RegistryEventHandler(IRequestManagement requests) {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        /// <inheritdoc/>
        public Task OnApplicationNewAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationUpdatedAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationDeletedAsync(OperationContextModel context,
            string applicationId, ApplicationInfoModel application) {
            return RemoveAllRequestsForEntityAsync(applicationId, context);
        }

        /// <inheritdoc/>
        public Task OnEndpointNewAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointActivatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointDeactivatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointUpdatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnEndpointDeletedAsync(OperationContextModel context,
            string endpointId, EndpointInfoModel endpoint) {
            return RemoveAllRequestsForEntityAsync(endpointId, context);
        }

        /// <summary>
        /// Delete all requests for the given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task RemoveAllRequestsForEntityAsync(string entityId,
            OperationContextModel context) {
            string nextPageLink = null;
            var result = await _requests.QueryRequestsAsync(
                new CertificateRequestQueryRequestModel {
                    EntityId = entityId
                }).ConfigureAwait(false);
            while (true) {
                nextPageLink = result.NextPageLink;
                foreach (var request in result.Requests) {
                    if (request.State != CertificateRequestState.Accepted) {
                        await Try.Async(() => _requests.AcceptRequestAsync(
                            request.RequestId, new VaultOperationContextModel {
                                AuthorityId = context?.AuthorityId,
                                Time = context?.Time ?? DateTime.UtcNow
                            })).ConfigureAwait(false);
                    }
                }
                if (result.NextPageLink == null) {
                    break;
                }
                result = await _requests.ListRequestsAsync(result.NextPageLink).ConfigureAwait(false);
            }
        }

        private readonly IRequestManagement _requests;
    }
}
