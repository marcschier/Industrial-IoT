// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Grains {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using Orleans;

    /// <summary>
    /// Implements data transfer services as actor grain
    /// </summary>
    public sealed class DataTransferGrain : Grain, IDataTransferGrain {


        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="transfer"></param>
        public DataTransferGrain(ITwinRegistry registry,
            ITransferServices<ConnectionModel> transfer) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _transfer = transfer ?? throw new ArgumentNullException(nameof(transfer));
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            ModelUploadStartRequestModel request, CancellationToken ct) {
            return await _transfer.ModelUploadStartAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public override async Task OnActivateAsync() {
            _twin = await _registry.GetTwinAsync(
                GrainReference.GrainIdentity.PrimaryKeyString).ConfigureAwait(true);
            await base.OnActivateAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public override Task OnDeactivateAsync() {
            _twin = null; // Clear twin
            return base.OnDeactivateAsync();
        }


        private readonly ITwinRegistry _registry;
        private readonly ITransferServices<ConnectionModel> _transfer;
        private TwinModel _twin;
    }
}
