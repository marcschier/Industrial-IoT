// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.IIoT.Extensions.Serializers;

    /// <summary>
    /// Enrollment group model extensions
    /// </summary>
    internal static class EnrollmentGroupModelEx {

        /// <summary>
        /// Create enrollment group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static EnrollmentGroupModel ToEnrollmentGroupModel(this EnrollmentGroup group,
            IJsonSerializer serializer) {
            if (group == null) {
                return null;
            }
            return new EnrollmentGroupModel {
                EnrollmentGroupId = group.EnrollmentGroupId,
                Registration = group.ToDeviceTwinRegistrationModel(serializer),
                Etag = group.ETag
            };
        }
    }
}
