// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Node services via twin model
    /// </summary>
    public interface INodeServices<T> {

        /// <summary>
        /// Read node value
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueReadResultModel> NodeValueReadAsync(T twin,
            ValueReadRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Write node value
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ValueWriteResultModel> NodeValueWriteAsync(T twin,
            ValueWriteRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Get meta data for method call (input and output arguments)
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            T twin, MethodMetadataRequestModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Call method
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodCallResultModel> NodeMethodCallAsync(T twin,
            MethodCallRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Read node attributes in batch
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ReadResultModel> NodeReadAsync(T twin,
            ReadRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Write node attributes in batch
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriteResultModel> NodeWriteAsync(T twin,
            WriteRequestModel request, CancellationToken ct = default);
    }
}
