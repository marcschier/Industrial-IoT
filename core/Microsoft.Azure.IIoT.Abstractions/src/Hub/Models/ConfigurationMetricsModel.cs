// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Azure IOT Configuration Metrics
    /// </summary>
    public class ConfigurationMetricsModel {

        /// <summary>
        /// Results of the metrics collection queries
        /// </summary>
        public IDictionary<string, long> Results { get; set; }

        /// <summary>
        /// Queries used for metrics collection
        /// </summary>
        public IDictionary<string, string> Queries { get; set; }
    }

}
