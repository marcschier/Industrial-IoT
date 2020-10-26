// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Api {
    using Microsoft.Azure.IIoT.Platform.Discovery.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery client api extensions
    /// </summary>
    public static class DiscoveryServiceApiEx {

        /// <summary>
        /// Find endpoints
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<EndpointInfoApiModel>> QueryAllEndpointsAsync(
            this IDiscoveryServiceApi service, EndpointInfoQueryApiModel query,
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
            this IDiscoveryServiceApi service, CancellationToken ct = default) {
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
        /// Find applications
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ApplicationInfoApiModel>> QueryAllApplicationsAsync(
            this IDiscoveryServiceApi service, ApplicationInfoQueryApiModel query,
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
            this IDiscoveryServiceApi service, CancellationToken ct = default) {
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
