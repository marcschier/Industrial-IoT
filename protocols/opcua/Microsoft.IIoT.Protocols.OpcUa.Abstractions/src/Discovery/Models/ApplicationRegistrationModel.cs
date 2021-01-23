// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Application with optional list of endpoints
    /// </summary>
    public class ApplicationRegistrationModel {

        /// <summary>
        /// Application information
        /// </summary>
        public ApplicationInfoModel Application { get; set; }

        /// <summary>
        /// List of endpoints for it
        /// </summary>
        public IReadOnlyList<EndpointInfoModel> Endpoints { get; set; }
    }
}
