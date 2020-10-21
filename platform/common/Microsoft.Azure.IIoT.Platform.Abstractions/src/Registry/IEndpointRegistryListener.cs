// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Notified when endpoint registry changes
    /// </summary>
    public interface IEndpointRegistryListener {

        /// <summary>
        /// New endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointNewAsync(OperationContextModel context,
            EndpointInfoModel endpoint);

        /// <summary>
        /// Updated endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointUpdatedAsync(OperationContextModel context,
            EndpointInfoModel endpoint);

        /// <summary>
        /// Deleted endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpointId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointDeletedAsync(OperationContextModel context,
            string endpointId, EndpointInfoModel endpoint);
    }
}
