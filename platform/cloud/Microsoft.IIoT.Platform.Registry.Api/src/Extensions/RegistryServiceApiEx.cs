// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api {
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry api extensions
    /// </summary>
    public static class RegistryServiceApiEx {

        /// <summary>
        /// List all sites
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> ListAllSitesAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var sites = new List<string>();
            var result = await service.ListSitesAsync(null, null, ct).ConfigureAwait(false);
            sites.AddRange(result.Sites);
            while (result.ContinuationToken != null) {
                result = await service.ListSitesAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                sites.AddRange(result.Sites);
            }
            return sites;
        }

        /// <summary>
        /// List all discoverers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<DiscovererApiModel>> ListAllDiscoverersAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<DiscovererApiModel>();
            var result = await service.ListDiscoverersAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListDiscoverersAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// Find discoverers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<DiscovererApiModel>> QueryAllDiscoverersAsync(
            this IRegistryServiceApi service, DiscovererQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<DiscovererApiModel>();
            var result = await service.QueryDiscoverersAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListDiscoverersAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// List all supervisors
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SupervisorApiModel>> ListAllSupervisorsAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<SupervisorApiModel>();
            var result = await service.ListSupervisorsAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListSupervisorsAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// Find supervisors
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SupervisorApiModel>> QueryAllSupervisorsAsync(
            this IRegistryServiceApi service, SupervisorQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<SupervisorApiModel>();
            var result = await service.QuerySupervisorsAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListSupervisorsAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// List all publishers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PublisherApiModel>> ListAllPublishersAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<PublisherApiModel>();
            var result = await service.ListPublishersAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListPublishersAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// Find publishers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PublisherApiModel>> QueryAllPublishersAsync(
            this IRegistryServiceApi service, PublisherQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<PublisherApiModel>();
            var result = await service.QueryPublishersAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListPublishersAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }


        /// <summary>
        /// List all gateways
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<GatewayApiModel>> ListAllGatewaysAsync(
            this IRegistryServiceApi service, CancellationToken ct = default) {
            var registrations = new List<GatewayApiModel>();
            var result = await service.ListGatewaysAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListGatewaysAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// Find gateways
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<GatewayApiModel>> QueryAllGatewaysAsync(
            this IRegistryServiceApi service, GatewayQueryApiModel query,
            CancellationToken ct = default) {
            var registrations = new List<GatewayApiModel>();
            var result = await service.QueryGatewaysAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListGatewaysAsync(result.ContinuationToken, null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }
    }
}
