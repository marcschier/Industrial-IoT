// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Platform.Registry.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Edge Gateway registry extensions
    /// </summary>
    public static class GatewayRegistryEx {

        /// <summary>
        /// Find edge gateway.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="gatewayId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<GatewayInfoModel> FindGatewayAsync(
            this IGatewayRegistry service, string gatewayId, CancellationToken ct = default) {
            try {
                return await service.GetGatewayAsync(gatewayId, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// List all sites
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<string>> ListAllSitesAsync(
            this IGatewayRegistry service, CancellationToken ct = default) {
            var sites = new List<string>();
            var result = await service.ListSitesAsync(null, null, ct).ConfigureAwait(false);
            sites.AddRange(result.Sites);
            while (result.ContinuationToken != null) {
                result = await service.ListSitesAsync(result.ContinuationToken, 
                    null, ct).ConfigureAwait(false);
                sites.AddRange(result.Sites);
            }
            return sites;
        }

        /// <summary>
        /// List all edge gateways
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<GatewayModel>> ListAllGatewaysAsync(
            this IGatewayRegistry service, CancellationToken ct = default) {
            var gateways = new List<GatewayModel>();
            var result = await service.ListGatewaysAsync(null, null, ct).ConfigureAwait(false);
            gateways.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListGatewaysAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                gateways.AddRange(result.Items);
            }
            return gateways;
        }

        /// <summary>
        /// Query all edge gateways
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<GatewayModel>> QueryAllGatewaysAsync(
            this IGatewayRegistry service, GatewayQueryModel query,
            CancellationToken ct = default) {
            var gateways = new List<GatewayModel>();
            var result = await service.QueryGatewaysAsync(query, null, ct).ConfigureAwait(false);
            gateways.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListGatewaysAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                gateways.AddRange(result.Items);
            }
            return gateways;
        }
    }
}
