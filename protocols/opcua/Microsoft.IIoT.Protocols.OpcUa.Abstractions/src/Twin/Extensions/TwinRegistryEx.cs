// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Exceptions;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Twin registry extensions
    /// </summary>
    public static class TwinRegistryEx {

        /// <summary>
        /// Get connection
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="twinId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<ConnectionModel> GetConnectionAsync(
            this ITwinRegistry registry, string twinId, CancellationToken ct = default) {
            if (registry is null) {
                throw new ArgumentNullException(nameof(registry));
            }
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var twin = await registry.GetTwinAsync(twinId, ct).ConfigureAwait(false);
            return twin.Connection;
        }

        /// <summary>
        /// Find twin.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="twinId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<TwinModel> FindTwinAsync(
            this ITwinRegistry service, string twinId, CancellationToken ct = default) {
            try {
                return await service.GetTwinAsync(twinId, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Find twins using query
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<TwinInfoModel>> QueryAllTwinsAsync(
            this ITwinRegistry service, TwinInfoQueryModel query,
            CancellationToken ct = default) {
            var registrations = new List<TwinInfoModel>();
            var result = await service.QueryTwinsAsync(query, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListTwinsAsync(result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }

        /// <summary>
        /// List all twins
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<TwinInfoModel>> ListAllTwinsAsync(
            this ITwinRegistry service, CancellationToken ct = default) {
            var registrations = new List<TwinInfoModel>();
            var result = await service.ListTwinsAsync(null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListTwinsAsync(result.ContinuationToken,
                     null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;
        }
    }
}
