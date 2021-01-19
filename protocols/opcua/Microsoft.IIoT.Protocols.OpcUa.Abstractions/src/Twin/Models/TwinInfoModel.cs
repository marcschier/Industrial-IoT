// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Twin info
    /// </summary>
    public class TwinInfoModel {

        /// <summary>
        /// Twin identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// User or null if anonymous authentication.
        /// </summary>
        public CredentialModel User { get; set; }

        /// <summary>
        /// Diagnostics configuration to use for the twin
        /// </summary>
        public DiagnosticsModel Diagnostics { get; set; }

        /// <summary>
        /// The operation timeout for this twin.
        /// </summary>
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// The last connection status of this twin
        /// </summary>
        public ConnectionStateModel ConnectionState { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        public OperationContextModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        public OperationContextModel Updated { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
