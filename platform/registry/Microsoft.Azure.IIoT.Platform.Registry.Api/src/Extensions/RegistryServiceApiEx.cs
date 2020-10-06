// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry api extensions
    /// </summary>
    public static class RegistryServiceApiEx {

        /// <summary>
        /// Find endpoints
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<EndpointInfoApiModel>> QueryAllEndpointsAsync(
            this IRegistryServiceApi service, EndpointInfoQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<EndpointInfoApiModel>();
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
        public static async Task<IEnumerable<EndpointInfoApiModel>> ListAllEndpointsAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<EndpointInfoApiModel>();
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
        public static async Task DeactivateEndpointAsync(this IRegistryServiceApi service,
            string endpointId, string generationId = null, CancellationToken ct = default) {
            if (string.IsNullOrEmpty(generationId)) {
                var ep = await service.GetEndpointAsync(endpointId, ct).ConfigureAwait(false);
                generationId = ep.GenerationId;
            }
            await service.UpdateEndpointAsync(endpointId,
                new EndpointInfoUpdateApiModel {
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
        public static async Task ActivateEndpointAsync(this IRegistryServiceApi service,
            string endpointId, string generationId = null, CancellationToken ct = default) {
            if (string.IsNullOrEmpty(generationId)) {
                var ep = await service.GetEndpointAsync(endpointId, ct).ConfigureAwait(false);
                generationId = ep.GenerationId;
            }
            await service.UpdateEndpointAsync(endpointId,
                new EndpointInfoUpdateApiModel {
                    GenerationId = generationId,
                    ActivationState = EntityActivationState.Activated
                }, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Find applications
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ApplicationInfoApiModel>> QueryAllApplicationsAsync(
            this IRegistryServiceApi service, ApplicationRegistrationQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<ApplicationInfoApiModel>();
            var result = await service.QueryApplicationsAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListApplicationsAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// List all applications
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ApplicationInfoApiModel>> ListAllApplicationsAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<ApplicationInfoApiModel>();
            var result = await service.ListApplicationsAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListApplicationsAsync(result.ContinuationToken, 
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }
    }
}
