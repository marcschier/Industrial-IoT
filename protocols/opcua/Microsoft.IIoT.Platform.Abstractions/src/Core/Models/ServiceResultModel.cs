// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Models {
    using Microsoft.IIoT.Extensions.Serializers;

    /// <summary>
    /// Service result
    /// </summary>
    public class ServiceResultModel {

        /// <summary>
        /// Error code - if null operation succeeded.
        /// </summary>
        public uint? StatusCode { get; set; }

        /// <summary>
        /// Error message in case of error or null.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Additional diagnostics information
        /// </summary>
        public VariantValue Diagnostics { get; set; }
    }
}
