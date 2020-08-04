// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Transfer services
    /// </summary>
    public interface ITransferServices<T> {

        /// <summary>
        /// Start exporting model to an endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns>file name of the model exported</returns>
        Task<ModelUploadStartResultModel> ModelUploadStartAsync(T endpoint,
            ModelUploadStartRequestModel request);
    }
}
