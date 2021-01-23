// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint repository extensions
    /// </summary>
    public static class EndpointRepositoryEx {

        /// <summary>
        /// Query all
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<EndpointInfoModel>> QueryAllAsync(
            this IEndpointRepository service,
            EndpointInfoQueryModel query = null,
            CancellationToken ct = default) {

            var registrations = new List<EndpointInfoModel>();
            var result = await service.QueryAsync(query, null, null, ct).ConfigureAwait(false);
            registrations.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.QueryAsync(query, result.ContinuationToken,
                    null, ct).ConfigureAwait(false);
                registrations.AddRange(result.Items);
            }
            return registrations;

        }
    }
}
