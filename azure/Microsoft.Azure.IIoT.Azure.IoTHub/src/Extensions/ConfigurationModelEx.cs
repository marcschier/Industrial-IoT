// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Configuration model extensions
    /// </summary>
    public static class ConfigurationModelEx {

        /// <summary>
        /// Convert configuration model to configuration
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Configuration ToConfiguration(this ConfigurationModel config) {
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            return new Configuration(config.Id) {
                Content = config.Content.ToContent(),
                ETag = config.Etag,
                Labels = config.Labels.Clone(),
                Priority = config.Priority,
                TargetCondition = config.TargetCondition
            };
        }

        /// <summary>
        /// Convert configuration to model
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ConfigurationModel ToModel(this Configuration config) {
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            return new ConfigurationModel {
                Id = config.Id,
                Etag = config.ETag,
                ContentType = config.ContentType,
                TargetCondition = config.TargetCondition,
                Priority = config.Priority,
                Labels = config.Labels.Clone(),
                Content = config.Content.ToModel(),
                CreatedTimeUtc = config.CreatedTimeUtc,
                LastUpdatedTimeUtc = config.LastUpdatedTimeUtc,
                Metrics = config.Metrics.ToModel(),
                SchemaVersion = config.SchemaVersion,
                SystemMetrics = config.SystemMetrics.ToModel()
            };
        }
    }
}
