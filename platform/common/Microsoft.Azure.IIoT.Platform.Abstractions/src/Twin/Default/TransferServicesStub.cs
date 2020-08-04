// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Stubbed out transfer functionality
    /// </summary>
    public sealed class TransferServicesStub<T> : ITransferServices<T> {

        /// <inheritdoc/>
        public Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            T endpoint, ModelUploadStartRequestModel request) {
            return Task.FromResult(new ModelUploadStartResultModel());
        }
    }
}
