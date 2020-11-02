// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Options;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Device Twin services extensions
    /// </summary>
    public static class DeviceTwinServicesEx {

        /// <summary>
        /// IoTHubOwner connection string to options helper
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static IOptions<IoTHubOptions> ToIoTHubOptions(this ConnectionString cs) {
            return Options.Create(new IoTHubOptions {
                IoTHubConnString = cs.ToString()
            });
        }

        /// <summary>
        /// Returns device connection string
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="primary"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<ConnectionString> GetConnectionStringAsync(
            this IDeviceTwinServices service, string deviceId, bool primary = true,
            CancellationToken ct = default) {
            var model = await service.GetRegistrationAsync(deviceId, null, ct).ConfigureAwait(false);
            if (model == null) {
                throw new ResourceNotFoundException("Could not find " + deviceId);
            }
            return ConnectionString.CreateDeviceConnectionString(service.HostName,
                deviceId, primary ?
                    model.Authentication.PrimaryKey : model.Authentication.SecondaryKey);
        }

        /// <summary>
        /// Returns module connection string
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="primary"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<ConnectionString> GetConnectionStringAsync(
            this IDeviceTwinServices service, string deviceId, string moduleId,
            bool primary = true, CancellationToken ct = default) {
            var model = await service.GetRegistrationAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            if (model == null) {
                throw new ResourceNotFoundException("Could not find " + moduleId);
            }
            return ConnectionString.CreateModuleConnectionString(service.HostName,
                deviceId, moduleId, primary ?
                    model.Authentication.PrimaryKey : model.Authentication.SecondaryKey);
        }
    }
}
