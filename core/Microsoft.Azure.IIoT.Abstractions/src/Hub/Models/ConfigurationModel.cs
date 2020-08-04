// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Configuration
    /// </summary>
    public class ConfigurationModel {

        /// <summary>
        /// Configuration Identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The etag
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Gets Schema version for the configuration
        /// </summary>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets labels for the configuration
        /// </summary>
        public IDictionary<string, string> Labels { get; set; }

        /// <summary>
        /// Gets or sets Content for the configuration
        /// </summary>
        public ConfigurationContentModel Content { get; set; }

        /// <summary>
        /// Gets the content type for configuration
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets Target Condition for the configuration
        /// </summary>
        public string TargetCondition { get; set; }

        /// <summary>
        /// Gets creation time for the configuration
        /// </summary>
        public DateTime CreatedTimeUtc { get; set; }

        /// <summary>
        /// Gets last update time for the configuration
        /// </summary>
        public DateTime LastUpdatedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Priority for the configuration
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// System Configuration Metrics
        /// </summary>
        public ConfigurationMetricsModel SystemMetrics { get; set; }

        /// <summary>
        /// Custom Configuration Metrics
        /// </summary>
        public ConfigurationMetricsModel Metrics { get; set; }
    }
}
