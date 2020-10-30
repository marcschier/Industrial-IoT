// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Data transfer grain
    /// </summary>
    public interface IDataTransferGrain : IGrainWithStringKey {

        /// <summary>
        /// Start model upload
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            ModelUploadStartRequestModel request, CancellationToken ct);
    }
}