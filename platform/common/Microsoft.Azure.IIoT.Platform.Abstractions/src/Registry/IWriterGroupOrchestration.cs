// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Orchestrates and places worker groups across publishers
    /// </summary>
    public interface IWriterGroupOrchestration {

        /// <summary>
        /// Place worker groups
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SynchronizeWriterGroupPlacementsAsync(
            CancellationToken ct = default);
    }
}