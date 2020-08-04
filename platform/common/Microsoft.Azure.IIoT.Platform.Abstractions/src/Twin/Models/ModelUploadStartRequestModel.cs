// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Model upload start request
    /// </summary>
    public class ModelUploadStartRequestModel {

        /// <summary>
        /// Optional diagnostics configuration
        /// </summary>
        public DiagnosticsModel Diagnostics { get; set; }

        /// <summary>
        /// Desired content type
        /// </summary>
        public string ContentMimeType { get; set; }

        /// <summary>
        /// Desired endpoint to http put result to
        /// </summary>
        public string UploadEndpointUrl { get; set; }

        /// <summary>
        /// Authorization header if required
        /// </summary>
        public string AuthorizationHeader { get; set; }
    }
}
