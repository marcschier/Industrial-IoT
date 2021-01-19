// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Clients {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Extensions.Orleans;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements transfer services as grain client
    /// </summary>
    public sealed class DataTransferGrainClient : ITransferServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="client"></param>
        public DataTransferGrainClient(IOrleansGrainClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            string twin, ModelUploadStartRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IDataTransferGrain>(twin);
            return await grain.ModelUploadStartAsync(request, ct).ConfigureAwait(true);
        }

        private readonly IOrleansGrainClient _client;
    }
}
