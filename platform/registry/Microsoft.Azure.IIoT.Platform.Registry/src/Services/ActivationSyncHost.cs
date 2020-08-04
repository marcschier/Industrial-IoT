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
    /// Performs continous endpoint activation synchronization
    /// </summary>
    public sealed class ActivationSyncHost : AbstractRunHost {

        /// <summary>
        /// Create activation process
        /// </summary>
        /// <param name="activation"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public ActivationSyncHost(IEndpointActivation activation, ILogger logger,
            IActivationSyncConfig config = null) : base(logger, "Endpoint synchronization",
                config?.ActivationSyncInterval ?? TimeSpan.FromMinutes(2)) {
            _activation = activation ?? throw new ArgumentNullException(nameof(activation));
        }

        /// <inheritdoc/>
        protected override Task RunAsync(CancellationToken token) {
            return _activation.SynchronizeActivationAsync(token);
        }

        private readonly IEndpointActivation _activation;
    }
}
