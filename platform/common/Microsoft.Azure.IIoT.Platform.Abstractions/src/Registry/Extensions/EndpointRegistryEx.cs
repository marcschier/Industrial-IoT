// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint registry extensions
    /// </summary>
    public static class EndpointRegistryEx {

        /// <summary>
        /// Find endpoint.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpointId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<EndpointInfoModel> FindEndpointAsync(
            this IEndpointRegistry service, string endpointId,
            CancellationToken ct = default) {
            try {
                return await service.GetEndpointAsync(endpointId, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Find endpoints using query
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<EndpointInfoModel>> QueryAllEndpointsAsync(
            this IEndpointRegistry service, EndpointInfoQueryModel query,
            CancellationToken ct = default) {
            var registrations = new List<EndpointInfoModel>();
            var result = await service.QueryEndpointsAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListEndpointsAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// List all endpoints
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<EndpointInfoModel>> ListAllEndpointsAsync(
            this IEndpointRegistry service, CancellationToken ct = default) {
            var registrations = new List<EndpointInfoModel>();
            var result = await service.ListEndpointsAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListEndpointsAsync(result.ContinuationToken,
                     null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// Deactivate an endpoint
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpointId"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task DeactivateEndpointAsync(this IEndpointRegistry service,
            string endpointId, string generationId = null, CancellationToken ct = default) {
            if (string.IsNullOrEmpty(generationId)) {
                var ep = await service.GetEndpointAsync(endpointId, ct).ConfigureAwait(false);
                generationId = ep.GenerationId;
            }
            await service.UpdateEndpointAsync(endpointId,
                new EndpointInfoUpdateModel {
                    GenerationId = generationId,
                    ActivationState = EntityActivationState.Deactivated
                }, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Activate an endpoint
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpointId"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task ActivateEndpointAsync(this IEndpointRegistry service,
            string endpointId, string generationId = null, CancellationToken ct = default) {
            if (string.IsNullOrEmpty(generationId)) {
                var ep = await service.GetEndpointAsync(endpointId, ct).ConfigureAwait(false);
                generationId = ep.GenerationId;
            }
            await service.UpdateEndpointAsync(endpointId,
                new EndpointInfoUpdateModel {
                    GenerationId = generationId,
                    ActivationState = EntityActivationState.Activated
                }, ct).ConfigureAwait(false);
        }

    }
}
