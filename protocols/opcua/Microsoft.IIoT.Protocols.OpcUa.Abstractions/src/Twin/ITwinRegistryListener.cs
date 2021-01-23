﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Notified when twin registry changes
    /// </summary>
    public interface ITwinRegistryListener {

        /// <summary>
        /// New twin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        Task OnTwinActivatedAsync(OperationContextModel context,
            TwinInfoModel twin);

        /// <summary>
        /// Updated twin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        Task OnTwinUpdatedAsync(OperationContextModel context,
            TwinInfoModel twin);

        /// <summary>
        /// Deactivated twin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        Task OnTwinDeactivatedAsync(OperationContextModel context,
            TwinInfoModel twin);
    }
}
