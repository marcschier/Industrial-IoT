﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
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
        /// Lost endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointLostAsync(OperationContextModel context,
            EndpointInfoModel endpoint);

        /// <summary>
        /// Lost endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointFoundAsync(OperationContextModel context,
            EndpointInfoModel endpoint);

        /// <summary>
        /// Deleted endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task OnEndpointDeletedAsync(OperationContextModel context,
            EndpointInfoModel endpoint);
    }
}
