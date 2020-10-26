// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery {
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Application registry change listener
    /// </summary>
    public interface IApplicationRegistryListener {

        /// <summary>
        /// Called when application is added
        /// </summary>
        /// <param name="context"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        Task OnApplicationNewAsync(OperationContextModel context,
            ApplicationInfoModel application);

        /// <summary>
        /// Called when application is updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        Task OnApplicationUpdatedAsync(OperationContextModel context,
            ApplicationInfoModel application);

        /// <summary>
        /// Lost application
        /// </summary>
        /// <param name="context"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        Task OnApplicationLostAsync(OperationContextModel context,
            ApplicationInfoModel application);

        /// <summary>
        /// Found application
        /// </summary>
        /// <param name="context"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        Task OnApplicationFoundAsync(OperationContextModel context,
            ApplicationInfoModel application);

        /// <summary>
        /// Called when application is unregistered
        /// </summary>
        /// <param name="context"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        Task OnApplicationDeletedAsync(OperationContextModel context,
            ApplicationInfoModel application);
    }
}
