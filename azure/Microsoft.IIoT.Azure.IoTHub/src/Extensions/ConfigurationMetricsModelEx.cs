// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Configuration metrics model extensions
    /// </summary>
    internal static class ConfigurationMetricsModelEx {

        /// <summary>
        /// Convert configuration metrics model to
        /// configuration metrics
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationMetrics ToConfigurationMetrics(this ConfigurationMetricsModel model) {
            if (model == null) {
                return null;
            }
            return new ConfigurationMetrics {
                Queries = model.Queries.Clone(),
                Results = model.Results.Clone()
            };
        }

        /// <summary>
        /// Convert configuration metrics to model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationMetricsModel ToConfigurationMetricsModel(this ConfigurationMetrics model) {
            if (model == null) {
                return null;
            }
            return new ConfigurationMetricsModel {
                Queries = model.Queries.Clone(),
                Results = model.Results.Clone()
            };
        }
    }
}
