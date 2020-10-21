// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
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
        /// <param name="twinId"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        Task OnTwinDeactivatedAsync(OperationContextModel context,
            string twinId, TwinInfoModel twin);
    }
}
