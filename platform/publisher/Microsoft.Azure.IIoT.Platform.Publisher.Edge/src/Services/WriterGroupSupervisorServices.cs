// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Services {
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Edge;
    using Microsoft.Azure.IIoT.Hosting;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Supervisor services allowing edge activating and managing writer group twin instances
    /// </summary>
    public class WriterGroupSupervisorServices : IActivationServices<string>, ISupervisorServices {

        /// <summary>
        /// Create supervisor
        /// </summary>
        /// <param name="hosts"></param>
        /// <param name="identity"></param>
        /// <param name="process"></param>
        /// <param name="logger"></param>
        public WriterGroupSupervisorServices(IModuleHostManager hosts, IIdentity identity,
            IProcessControl process, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        /// <inheritdoc/>
        public async Task ActivateAsync(string id, string secret, CancellationToken ct) {
            await _hosts.StartAsync(id, secret, ct);
        }

        /// <inheritdoc/>
        public async Task DeactivateAsync(string id, CancellationToken ct) {
            await _hosts.StopAsync(id, ct);
        }

        /// <inheritdoc/>
        public async Task AttachAsync(string id, string secret) {
            await _hosts.QueueStartAsync(id, secret);
        }

        /// <inheritdoc/>
        public async Task DetachAsync(string id) {
            await _hosts.QueueStopAsync(id);
        }

        /// <inheritdoc/>
        public Task<SupervisorStatusModel> GetStatusAsync(CancellationToken ct) {
            var entities = _hosts.Hosts
                .Select(h => new EntityActivationStatusModel {
                    Id = h.Item1,
                    ActivationState = h.Item2 ?
                        EntityActivationState.ActivatedAndConnected :
                        EntityActivationState.Activated
                });
            var status = new SupervisorStatusModel {
                Entities = entities.ToList(),
                DeviceId = _identity.DeviceId,
                ModuleId = _identity.ModuleId
            };
            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task ResetAsync(CancellationToken ct) {
            _process.Reset();
            return Task.CompletedTask;
        }

        private readonly ILogger _logger;
        private readonly IModuleHostManager _hosts;
        private readonly IIdentity _identity;
        private readonly IProcessControl _process;
    }
}

