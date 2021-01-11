// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub {
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Device provisioning group enrollment services
    /// </summary>
    public interface IGroupEnrollmentServices {

        /// <summary>
        /// Create enrollment group
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="attestation"></param>
        /// <param name="registration"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EnrollmentGroupModel> CreateEnrollmentGroupAsync(string enrollmentGroupId,
            AuthenticationModel attestation, DeviceRegistrationModel registration = null,
            CancellationToken ct = default);

        /// <summary>
        /// Create enrollment group
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="certificate"></param>
        /// <param name="registration"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EnrollmentGroupModel> CreateEnrollmentGroupAsync(string enrollmentGroupId,
            X509Certificate2 certificate, DeviceRegistrationModel registration = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get enrollment group information
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EnrollmentGroupModel> GetEnrollmentGroupAsync(string enrollmentGroupId,
            CancellationToken ct = default);

        /// <summary>
        /// Query enrollment group registrations
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="query"></param>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DeviceRegistrationListModel> QueryEnrollmentGroupRegistrationsAsync(
            string enrollmentGroupId, string query = null, string continuation = null,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Query enrollment groups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EnrollmentGroupListModel> QueryEnrollmentGroupsAsync(
            string query = null, string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Delete enrollment group
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteEnrollmentGroupAsync(string enrollmentGroupId,
            CancellationToken ct = default);
    }
}