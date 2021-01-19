// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery result handling
    /// </summary>
    public interface IDiscoveryResultHandler {

        /// <summary>
        /// Report discovery results
        /// </summary>
        /// <param name="results"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ReportResultsAsync(IEnumerable<DiscoveryResultModel> results,
            CancellationToken ct = default);
    }
}