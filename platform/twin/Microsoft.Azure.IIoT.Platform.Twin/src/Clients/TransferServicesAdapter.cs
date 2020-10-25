﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements transfer services as adapter through endpoint registry
    /// </summary>
    public sealed class TransferServicesAdapter : ITransferServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="transfer"></param>
        public TransferServicesAdapter(ITwinRegistry registry,
            ITransferServices<ConnectionModel> transfer) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _transfer = transfer ?? throw new ArgumentNullException(nameof(transfer));
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            string twin, ModelUploadStartRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _transfer.ModelUploadStartAsync(conn, request, ct).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _registry;
        private readonly ITransferServices<ConnectionModel> _transfer;
    }
}