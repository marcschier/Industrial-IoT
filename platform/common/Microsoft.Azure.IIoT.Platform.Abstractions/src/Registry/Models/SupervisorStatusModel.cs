// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Supervisor runtime status
    /// </summary>
    public class SupervisorStatusModel {

        /// <summary>
        /// Gateway device id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Module id
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Entity diagnostics
        /// </summary>
        public List<EntityActivationStatusModel> Entities { get; set; }
    }
}
