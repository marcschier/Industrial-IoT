// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Performs continous publisher placement of writer groups
    /// </summary>
    public sealed class OrchestrationHost : AbstractRunHost {

        /// <summary>
        /// Create process
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public OrchestrationHost(IWriterGroupOrchestration orchestrator,
            ILogger logger, IWriterGroupOrchestrationConfig config = null) :
            base(logger, "Publisher Orchestration",
                config?.UpdatePlacementInterval ?? TimeSpan.FromMinutes(3)) {
            _orchestrator = orchestrator ??
                throw new ArgumentNullException(nameof(orchestrator));
        }

        /// <inheritdoc/>
        protected override Task RunAsync(CancellationToken token) {
            return _orchestrator.SynchronizeWriterGroupPlacementsAsync(token);
        }

        private readonly IWriterGroupOrchestration _orchestrator;
    }
}
