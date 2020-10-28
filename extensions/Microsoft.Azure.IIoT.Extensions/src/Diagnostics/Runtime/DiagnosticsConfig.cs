// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    public class DiagnosticsConfig : ConfigBase, IDiagnosticsConfig {

        /// <summary>
        /// Configuration keys
        /// </summary>
        private const string kDiagnosticsLevelKey = "Diagnostics:DiagnosticsLevel";
        private const string kMetricsCollectionIntervalKey = "Diagnostics:MetricsCollectionInterval";

        /// <inheritdoc/>
        public DiagnosticsLevel DiagnosticsLevel => (DiagnosticsLevel)
            GetIntOrDefault(kDiagnosticsLevelKey,
                () => GetIntOrDefault(PcsVariable.PCS_DIAGNOSTICS_LEVEL,
                () => 0));
        /// <inheritdoc/>
        public TimeSpan? MetricsCollectionInterval =>
            GetDurationOrNull(kMetricsCollectionIntervalKey);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public DiagnosticsConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
