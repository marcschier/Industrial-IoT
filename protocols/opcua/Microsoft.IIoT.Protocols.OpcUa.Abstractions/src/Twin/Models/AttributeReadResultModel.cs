// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;

    /// <summary>
    /// Attribute value read
    /// </summary>
    public class AttributeReadResultModel {

        /// <summary>
        /// Attribute value
        /// </summary>
        public VariantValue Value { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
