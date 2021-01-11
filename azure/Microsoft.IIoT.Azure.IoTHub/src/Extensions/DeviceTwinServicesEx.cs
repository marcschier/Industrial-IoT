// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Linq;
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
        public static IOptions<IoTHubServiceOptions> ToIoTHubOptions(this ConnectionString cs) {
            return Options.Create(new IoTHubServiceOptions {
                ConnectionString = cs.ToString()
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

        /// <summary>
        /// Find twin or return null
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<DeviceTwinModel> FindAsync(this IDeviceTwinServices service,
            string deviceId, string moduleId = null, CancellationToken ct = default) {
            try {
                return await service.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            }
            catch (ResourceNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Returns device or module primary key
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<string> GetPrimaryKeyAsync(
            this IDeviceTwinServices service, string deviceId, string moduleId = null,
            CancellationToken ct = default) {
            var model = await service.GetRegistrationAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            if (model == null) {
                throw new ResourceNotFoundException("Could not find " + deviceId);
            }
            return model.Authentication.PrimaryKey;
        }

        /// <summary>
        /// Returns device or module secondary key
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<string> GetSecondaryKeyAsync(
            this IDeviceTwinServices service, string deviceId, string moduleId = null,
            CancellationToken ct = default) {
            var model = await service.GetRegistrationAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            if (model == null) {
                throw new ResourceNotFoundException("Could not find " + deviceId);
            }
            return model.Authentication.SecondaryKey;
        }

        /// <summary>
        /// Query twins
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<DeviceTwinListModel> QueryDeviceTwinsAsync(
            this IDeviceTwinServices service, string query, string continuation,
            int? pageSize = null, CancellationToken ct = default) {
            var response = await service.QueryAsync(query, continuation, pageSize, ct).ConfigureAwait(false);
            return new DeviceTwinListModel {
                ContinuationToken = response.ContinuationToken,
                Items = response.Result
                    .Select(j => j.ConvertTo<DeviceTwinModel>())
                    .ToList()
            };
        }

        /// <summary>
        /// Query hub for device twins
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<List<DeviceTwinModel>> QueryAllDeviceTwinsAsync(
            this IDeviceTwinServices service, string query, CancellationToken ct = default) {
            var result = new List<DeviceTwinModel>();
            string continuation = null;
            do {
                var response = await service.QueryDeviceTwinsAsync(query, continuation, null, ct).ConfigureAwait(false);
                result.AddRange(response.Items);
                continuation = response.ContinuationToken;
            }
            while (continuation != null);
            return result;
        }

        /// <summary>
        /// Update device property through twin
        /// </summary>
        /// <param name="service"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task UpdatePropertyAsync(this IDeviceTwinServices service,
            string deviceId, string moduleId, string property, VariantValue value,
            CancellationToken ct = default) {
            return service.UpdatePropertiesAsync(deviceId, moduleId,
                new Dictionary<string, VariantValue> {
                    [property] = value ?? VariantValue.Null
                }, null, ct);
        }
    }
}
