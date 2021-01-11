// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Configuration model extensions
    /// </summary>
    internal static class ConfigurationModelEx {

        /// <summary>
        /// Convert configuration model to configuration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Configuration ToConfiguration(this ConfigurationModel model) {
            if (model == null) {
                return null;
            }
            return new Configuration(model.Id) {
                Content = model.Content.ToConfigurationContent(),
                ETag = model.Etag,
                Labels = model.Labels.Clone(),
                Priority = model.Priority,
                TargetCondition = model.TargetCondition
            };
        }

        /// <summary>
        /// Convert configuration to model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationModel ToConfigurationModel(this Configuration model) {
            if (model == null) {
                return null;
            }
            return new ConfigurationModel {
                Id = model.Id,
                Etag = model.ETag,
                ContentType = model.ContentType,
                TargetCondition = model.TargetCondition,
                Priority = model.Priority,
                Labels = model.Labels.Clone(),
                Content = model.Content.ToConfigurationContentModel(),
                CreatedTimeUtc = model.CreatedTimeUtc,
                LastUpdatedTimeUtc = model.LastUpdatedTimeUtc,
                Metrics = model.Metrics.ToConfigurationMetricsModel(),
                SchemaVersion = model.SchemaVersion,
                SystemMetrics = model.SystemMetrics.ToConfigurationMetricsModel()
            };
        }
    }
}
