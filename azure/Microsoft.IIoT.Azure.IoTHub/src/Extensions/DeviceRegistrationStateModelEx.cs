// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices.Provisioning.Service;

    /// <summary>
    /// Device registration model extensions
    /// </summary>
    internal static class DeviceRegistrationStateModelEx {

        /// <summary>
        /// Create device registration model
        /// </summary>
        /// <param name="state"></param>
        /// <param name="enrollmentGroupId"></param>
        /// <returns></returns>
        public static DeviceRegistrationStateModel ToDeviceRegistrationModel(
            this DeviceRegistrationState state, string enrollmentGroupId) {
            if (state == null) {
                return null;
            }
            return new DeviceRegistrationStateModel {
                EnrollmentGroupId = enrollmentGroupId,
                RegistrationId = state.RegistrationId,
                Hub = state.AssignedHub,
                Id = state.DeviceId,
                Status = state.Status.ToString(),
                Etag = state.ETag
            };
        }
    }
}
