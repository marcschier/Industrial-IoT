// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Twin properties
    /// </summary>
    public class TwinPropertiesModel {

        /// <summary>
        /// Reported settings
        /// </summary>
        public IReadOnlyDictionary<string, VariantValue> Reported { get; set; }

        /// <summary>
        /// Desired settings
        /// </summary>
        public IReadOnlyDictionary<string, VariantValue> Desired { get; set; }
    }
}
