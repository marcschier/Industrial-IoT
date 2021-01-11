// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows to enable or disabling the ability to enroll
    /// child devices using the device or modules credentials.
    /// There is a maximum number of 100 enrollment groups
    /// which means only 100 parent devices can exist.
    /// </summary>
    public interface IChildEnrollmentControl {

        /// <summary>
        /// Allows child enrollment
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AllowChildEnrollmentAsync(string deviceId,
            string moduleId = null, CancellationToken ct = default);

        /// <summary>
        /// Check whether child enrollment is enabled
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> IsChildEnrollmentCapableAsync(string deviceId,
            string moduleId = null, CancellationToken ct = default);

        /// <summary>
        /// Disables child enrollment
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DenyChildEnrollmentAsync(string deviceId,
            string moduleId = null, CancellationToken ct = default);
    }
}