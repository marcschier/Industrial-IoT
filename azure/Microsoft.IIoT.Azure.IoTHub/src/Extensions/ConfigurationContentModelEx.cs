// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Configuration content model extensions
    /// </summary>
    public static class ConfigurationContentModelEx {

        /// <summary>
        /// Convert configuration content model to
        /// configuration content
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationContent ToConfigurationContent(this ConfigurationContentModel model) {
            if (model == null) {
                return null;
            }
            return new ConfigurationContent {
                ModulesContent = model.ModulesContent
                    .ToDictionary(k => k.Key, v => (IDictionary<string, object>)v.Value.Clone()),
                DeviceContent = model.DeviceContent.Clone()
            };
        }

        /// <summary>
        /// Convert configuration content to model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ConfigurationContentModel ToConfigurationContentModel(this ConfigurationContent model) {
            if (model == null) {
                return null;
            }
            return new ConfigurationContentModel {
                ModulesContent = model.ModulesContent
                    .ToDictionary(k => k.Key, v => (IReadOnlyDictionary<string, object>)v.Value.Clone()),
                DeviceContent = model.DeviceContent.Clone()
            };
        }
    }
}
