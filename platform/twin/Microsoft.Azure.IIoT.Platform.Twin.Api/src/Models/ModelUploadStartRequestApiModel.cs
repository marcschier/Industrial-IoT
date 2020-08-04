// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Model upload request model
    /// </summary>
    [DataContract]
    public class ModelUploadStartRequestApiModel {

        /// <summary>
        /// Optional diagnostics configuration
        /// </summary>
        [DataMember(Name = "diagnostics", Order = 0,
            EmitDefaultValue = false)]
        public DiagnosticsApiModel Diagnostics { get; set; }

        /// <summary>
        /// Desired content type
        /// </summary>
        [DataMember(Name = "contentMimeType", Order = 1,
            EmitDefaultValue = false)]
        public string ContentMimeType { get; set; }

        /// <summary>
        /// Desired endpoint to http put result to
        /// </summary>
        [DataMember(Name = "uploadEndpointUrl", Order = 2,
            EmitDefaultValue = false)]
        public string UploadEndpointUrl { get; set; }

        /// <summary>
        /// Authorization header if required
        /// </summary>
        [DataMember(Name = "authorizationHeader", Order = 3,
            EmitDefaultValue = false)]
        public string AuthorizationHeader { get; set; }
    }
}
