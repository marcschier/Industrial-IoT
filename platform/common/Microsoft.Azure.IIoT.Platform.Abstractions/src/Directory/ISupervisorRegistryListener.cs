// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory {
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Supervisor registry change listener
    /// </summary>
    public interface ISupervisorRegistryListener {

        /// <summary>
        /// Called when supervisor is created
        /// </summary>
        /// <param name="context"></param>
        /// <param name="supervisor"></param>
        /// <returns></returns>
        Task OnSupervisorNewAsync(DirectoryOperationContextModel context,
            SupervisorModel supervisor);

        /// <summary>
        /// Called when supervisor is updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="supervisor"></param>
        /// <returns></returns>
        Task OnSupervisorUpdatedAsync(DirectoryOperationContextModel context,
            SupervisorModel supervisor);

        /// <summary>
        /// Called when supervisor is deleted
        /// </summary>
        /// <param name="context"></param>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        Task OnSupervisorDeletedAsync(DirectoryOperationContextModel context,
            string supervisorId);
    }
}
