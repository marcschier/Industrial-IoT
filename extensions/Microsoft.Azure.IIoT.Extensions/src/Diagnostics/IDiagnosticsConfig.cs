// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using System;

    /// <summary>
    /// Diagnostic configuration
    /// </summary>
    public interface IDiagnosticsConfig {

        /// <summary>
        /// Level of diagnostics
        /// </summary>
        DiagnosticsLevel DiagnosticsLevel { get; }

        /// <summary>
        /// Metrics collection interval if configured
        /// </summary>
        TimeSpan? MetricsCollectionInterval { get; }
    }
}
