// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub {
    using Microsoft.Azure.IIoT.Hub.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration services extensions
    /// </summary>
    public static class DeviceDeploymentServicesEx {

        /// <summary>
        /// Delete configuration
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configurationId"></param>
        /// <returns></returns>
        public static Task DeleteConfigurationAsync(
            this IDeviceDeploymentServices service, string configurationId) {
            return service.DeleteConfigurationAsync(configurationId, null);
        }

        /// <summary>
        /// Delete configuration
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Task DeleteConfigurationAsync(
            this IDeviceDeploymentServices service, ConfigurationModel configuration) {
            return service.DeleteConfigurationAsync(configuration.Id, configuration.Etag);
        }

        /// <summary>
        /// List all configurations
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static Task<IEnumerable<ConfigurationModel>> ListConfigurationsAsync(
            this IDeviceDeploymentServices service) {
            return service.ListConfigurationsAsync(null);
        }
    }
}
