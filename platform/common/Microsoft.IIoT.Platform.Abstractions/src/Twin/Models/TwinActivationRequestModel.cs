// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Twin activation request
    /// </summary>
    public class TwinActivationRequestModel {

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Optional twin identifier if different from endpoint id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User for user authentication
        /// </summary>
        public CredentialModel User { get; set; }

        /// <summary>
        /// Diagnostics
        /// </summary>
        public DiagnosticsModel Diagnostics { get; set; }

        /// <summary>
        /// The operation timeout to create sessions.
        /// </summary>
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// Operation audit context
        /// </summary>
        public OperationContextModel Context { get; set; }
    }
}

