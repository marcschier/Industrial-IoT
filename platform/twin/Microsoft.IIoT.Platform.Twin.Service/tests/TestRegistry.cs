// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Service {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    public interface ITestRegistry {

        /// <summary>
        /// Connection
        /// </summary>
        ConnectionModel Connection { get; set; }
    }

    /// <summary>
    /// Test twin module
    /// </summary>
    public sealed class TestRegistry : ITwinRegistry, ITestRegistry {

        /// <inheritdoc/>
        public ConnectionModel Connection { get; set; }

        /// <inheritdoc/>
        public Task<TwinActivationResultModel> ActivateTwinAsync(
            TwinActivationRequestModel request, OperationContextModel context, 
            CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<TwinModel> GetTwinAsync(string twinId,
            CancellationToken ct) {
            if (Connection == null) {
                throw new NotImplementedException();
            }
            return Task.FromResult(new TwinModel {
                Connection = Connection
            });
        }

        /// <inheritdoc/>
        public Task<TwinInfoListModel> ListTwinsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<TwinInfoListModel> QueryTwinsAsync(TwinInfoQueryModel query,
            int? pageSize, CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task UpdateTwinAsync(string twinId, TwinInfoUpdateModel model, 
            OperationContextModel context, CancellationToken ct) {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DeactivateTwinAsync(string twinId, string generationId,
            OperationContextModel context, CancellationToken ct) {
            throw new NotImplementedException();
        }
    }
}
