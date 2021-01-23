// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Server registration request
    /// </summary>
    public class ServerRegistrationRequestModel {

        /// <summary>
        /// Discovery url to use for registration
        /// </summary>
        public string DiscoveryUrl { get; set; }

        /// <summary>
        /// User defined request id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Operation audit context
        /// </summary>
        public OperationContextModel Context { get; set; }
    }
}
