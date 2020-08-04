// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Orchestrator configuration
    /// </summary>
    public class OrchestrationConfig : ConfigBase, IWriterGroupOrchestrationConfig {

        /// <summary>
        /// Keys
        /// </summary>
        private const string kUpdateIntervalKey = "UpdatePlacementInterval";

        /// <inheritdoc/>
        public TimeSpan? UpdatePlacementInterval => GetDurationOrNull(kUpdateIntervalKey);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="configuration"></param>
        public OrchestrationConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}