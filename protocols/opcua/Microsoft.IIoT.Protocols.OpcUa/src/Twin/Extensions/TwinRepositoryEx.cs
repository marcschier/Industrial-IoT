// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin {
    using Microsoft.IIoT.Platform.Twin.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin repository extensions
    /// </summary>
    public static class TwinRepositoryEx {

        /// <summary>
        /// Query all
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TwinInfoModel>> QueryAllAsync(
            this ITwinRepository service, TwinInfoQueryModel query = null,
            CancellationToken ct = default) {

            var registrations = new List<TwinInfoModel>();
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
