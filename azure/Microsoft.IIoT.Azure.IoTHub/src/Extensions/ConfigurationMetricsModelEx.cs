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
    public static class ConfigurationMetricsModelEx {

        /// <summary>
        /// Convert configuration metrics model to
        /// configuration metrics
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationMetrics ToContent(this ConfigurationMetricsModel model) {
            if (model is null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new ConfigurationMetrics {
                Queries = model.Queries.Clone(),
                Results = model.Results.Clone()
            };
        }

        /// <summary>
        /// Convert configuration metrics to model
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static ConfigurationMetricsModel ToModel(this ConfigurationMetrics content) {
            if (content is null) {
                throw new ArgumentNullException(nameof(content));
            }
            return new ConfigurationMetricsModel {
                Queries = content.Queries.Clone(),
                Results = content.Results.Clone()
            };
        }
    }
}
