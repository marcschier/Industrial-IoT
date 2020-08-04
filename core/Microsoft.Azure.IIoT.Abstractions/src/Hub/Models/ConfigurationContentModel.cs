// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Configuration
    /// </summary>
    public class ConfigurationContentModel {

        /// <summary>
        /// Gets or sets modules configurations
        /// </summary>
        public IDictionary<string, IDictionary<string, object>> ModulesContent { get; set; }

        /// <summary>
        /// Gets or sets device configuration
        /// </summary>
        public IDictionary<string, object> DeviceContent { get; set; }
    }
}
