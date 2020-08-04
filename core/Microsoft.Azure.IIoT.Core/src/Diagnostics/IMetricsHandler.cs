// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle metrics reporting
    /// </summary>
    public interface IMetricsHandler {

        /// <summary>
        /// Called on start
        /// </summary>
        void OnStarting();

        /// <summary>
        /// Push metrics
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PushAsync(Stream stream, CancellationToken ct);

        /// <summary>
        /// Called on stop
        /// </summary>
        void OnStopped();
    }
}