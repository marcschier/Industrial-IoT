// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using System;

    /// <summary>
    /// Orchestration configuration
    /// </summary>
    public interface IWriterGroupOrchestrationConfig {

        /// <summary>
        /// Update group placement interval
        /// </summary>
        TimeSpan? UpdatePlacementInterval { get; }
    }
}
