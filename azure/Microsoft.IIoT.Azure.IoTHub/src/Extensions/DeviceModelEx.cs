// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices;
    using System;

    /// <summary>
    /// Device model extensions
    /// </summary>
    public static class DeviceModelEx {

        /// <summary>
        /// Check whether device is connected
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool? IsConnected(this DeviceModel model) {
            if (model is null) {
                return null;
            }
            return model.ConnectionState?.Equals("Connected",
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Check whether device is connected
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool? IsDisabled(this DeviceModel model) {
            if (model is null) {
                return null;
            }
            return model.Status?.Equals("disabled",
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Clone twin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DeviceModel Clone(this DeviceModel model) {
            if (model == null) {
                return null;
            }
            return new DeviceModel {
                Etag = model.Etag,
                Id = model.Id,
                Status = model.IsConnected() == true ? "connected" : "disconnected",
                ModuleId = model.ModuleId,
                ConnectionState = model.ConnectionState,
                Authentication = model.Authentication.Clone()
            };
        }

        /// <summary>
        /// Clone authentication
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DeviceAuthenticationModel Clone(this DeviceAuthenticationModel model) {
            if (model == null) {
                return null;
            }
            return new DeviceAuthenticationModel {
                PrimaryKey = model.PrimaryKey,
                SecondaryKey = model.SecondaryKey
            };
        }

        /// <summary>
        /// Convert twin to module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="hub"></param>
        /// <returns></returns>
        public static DeviceModel ToModel(this Module module, string hub) {
            return new DeviceModel {
                Id = module.DeviceId,
                Hub = hub,
                ModuleId = module.Id,
                Status = "enabled",
                DeviceScope = null,
                ConnectionState = module.ConnectionState.ToString(),
                Authentication = module.Authentication.ToModel(),
                Etag = module.ETag
            };
        }

        /// <summary>
        /// Convert twin to module
        /// </summary>
        /// <param name="device"></param>
        /// <param name="hub"></param>
        /// <returns></returns>
        public static DeviceModel ToModel(this Device device, string hub) {
            return new DeviceModel {
                Id = device.Id,
                Hub = hub,
                ModuleId = null,
                DeviceScope = device.Scope,
                Status = device.Status.ToString().ToLowerInvariant(),
                ConnectionState = device.ConnectionState.ToString(),
                Authentication = device.Authentication.ToModel(),
                Etag = device.ETag
            };
        }

        /// <summary>
        /// Convert twin to module
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public static DeviceAuthenticationModel ToModel(this AuthenticationMechanism auth) {
            return new DeviceAuthenticationModel {
                PrimaryKey = auth.SymmetricKey.PrimaryKey,
                SecondaryKey = auth.SymmetricKey.SecondaryKey
            };
        }
    }
}
