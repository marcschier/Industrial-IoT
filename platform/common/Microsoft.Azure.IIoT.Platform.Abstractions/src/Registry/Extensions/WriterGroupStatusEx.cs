// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer group status extensions
    /// </summary>
    public static class WriterGroupStatusEx {

        /// <summary>
        /// List status of activated and connected writer groups
        /// </summary>
        /// <param name="service"></param>
        /// <param name="onlyConnected"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<EntityActivationStatusModel>> ListAllWriterGroupActivationsAsync(
            this IWriterGroupStatus service, bool onlyConnected = true, CancellationToken ct = default) {
            var supervisors = new List<EntityActivationStatusModel>();
            var result = await service.ListWriterGroupActivationsAsync(null, onlyConnected, null, ct);
            supervisors.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.ListWriterGroupActivationsAsync(result.ContinuationToken,
                    onlyConnected, null, ct);
                supervisors.AddRange(result.Items);
            }
            return supervisors;
        }
    }
}