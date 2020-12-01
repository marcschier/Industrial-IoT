// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Connection update request
    /// </summary>
    public class TwinInfoUpdateModel {

        /// <summary>
        /// Generation Id to match
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// User
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
    }
}
