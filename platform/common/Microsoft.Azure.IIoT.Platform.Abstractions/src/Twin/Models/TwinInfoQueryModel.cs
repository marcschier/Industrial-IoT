// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Connection query
    /// </summary>
    public class TwinInfoQueryModel {

        /// <summary>
        /// Endpoint id
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Credential type for the connection 
        /// </summary>
        public CredentialType? Credential { get; set; }

        /// <summary>
        /// The last state of the the connection
        /// </summary>
        public ConnectionStatus? State { get; set; }
    }
}

