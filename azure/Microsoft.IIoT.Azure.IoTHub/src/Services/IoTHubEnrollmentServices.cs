// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Services {
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides enrollment services for edge workloads.  Registers or unregisters an
    /// enrollment group for a device allowing it to register devices using its
    /// credentials.  For now only symmetric key credentials are supported.
    /// </summary>
    public sealed class IoTHubEnrollmentServices : IChildEnrollmentControl {

        /// <summary>
        /// Create enrollment service
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="groups"></param>
        public IoTHubEnrollmentServices(IDeviceTwinServices iothub, IGroupEnrollmentServices groups) {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }

        /// <inheritdoc/>
        public async Task AllowChildEnrollmentAsync(string deviceId, string moduleId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }

            string deviceScope = null;
            var registration = await _iothub.GetRegistrationAsync(deviceId, moduleId,
                ct).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(moduleId)) {
                // Add device scope of the gateway to connect through it
                var gateway = await _iothub.GetRegistrationAsync(deviceId, null,
                    ct).ConfigureAwait(false);
                deviceScope = gateway.Scope;
            }

            // Create or update enrollment group for the device
            var enrollmentGroupId = GetEnrollmentGroupId(deviceId, moduleId);
            await _groups.CreateEnrollmentGroupAsync(enrollmentGroupId, registration.Authentication,
                new DeviceRegistrationModel {
                    DeviceScope = deviceScope,
                    Hub = registration.Hub,
                    Properties = new Dictionary<string, VariantValue> {
                        [nameof(enrollmentGroupId)] = enrollmentGroupId,
                        [nameof(deviceId)] = deviceId,
                        [nameof(moduleId)] = moduleId,
                    }
                }, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> IsChildEnrollmentCapableAsync(string deviceId,
            string moduleId, CancellationToken ct) {
            try {
                var enrollmentGroupId = GetEnrollmentGroupId(deviceId, moduleId);
                var group = await _groups.GetEnrollmentGroupAsync(enrollmentGroupId,
                    ct).ConfigureAwait(false);
                return group != null;
            }
            catch (ResourceNotFoundException) {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task DenyChildEnrollmentAsync(string deviceId, string moduleId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }
            // Delete enrollment group
            var enrollmentGroupId = GetEnrollmentGroupId(deviceId, moduleId);
            await _groups.DeleteEnrollmentGroupAsync(enrollmentGroupId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create enrollment group id
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        private static string GetEnrollmentGroupId(string deviceId, string moduleId) {
            return ("enrollment" + deviceId + (moduleId ?? "")).ToSha256Hash().ToLowerInvariant();
        }

        private readonly IDeviceTwinServices _iothub;
        private readonly IGroupEnrollmentServices _groups;
    }
}
