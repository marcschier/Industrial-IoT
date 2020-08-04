// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Query status of writer groups
    /// </summary>
    public interface IWriterGroupStatus {

        /// <summary>
        /// List status of activated and connected writer groups
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="onlyConnected"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EntityActivationStatusListModel> ListWriterGroupActivationsAsync(
            string continuation = null, bool onlyConnected = true,
            int? pageSize = null, CancellationToken ct = default);
    }
}