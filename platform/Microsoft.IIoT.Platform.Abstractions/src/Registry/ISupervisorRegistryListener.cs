﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Core.Models;
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
        Task OnSupervisorNewAsync(OperationContextModel context,
            SupervisorModel supervisor);

        /// <summary>
        /// Called when supervisor is updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="supervisor"></param>
        /// <returns></returns>
        Task OnSupervisorUpdatedAsync(OperationContextModel context,
            SupervisorModel supervisor);

        /// <summary>
        /// Called when supervisor is deleted
        /// </summary>
        /// <param name="context"></param>
        /// <param name="supervisorId"></param>
        /// <returns></returns>
        Task OnSupervisorDeletedAsync(OperationContextModel context,
            string supervisorId);
    }
}