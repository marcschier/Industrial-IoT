// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Clients {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using ServiceClient = Microsoft.Azure.Devices.Provisioning.Service.ProvisioningServiceClient;

    /// <summary>
    /// Implementation of provisioning services using DPS service sdk.
    /// </summary>
    public sealed class ProvisioningServiceClient : IGroupEnrollmentServices {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public ProvisioningServiceClient(IOptions<ProvisioningServiceOptions> options,
            IJsonSerializer serializer, ILogger logger) {
            if (string.IsNullOrEmpty(options.Value.ConnectionString)) {
                throw new ArgumentException("Missing connection string", nameof(options));
            }

            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = ServiceClient.CreateFromConnectionString(
                options.Value.ConnectionString);
        }

        /// <inheritdoc/>
        public async Task<EnrollmentGroupListModel> QueryEnrollmentGroupsAsync(
            string query, string continuation, int? pageSize, CancellationToken ct) {
            try {
                if (continuation != null) {
                    _serializer.DeserializeContinuationToken(continuation,
                        out query, out continuation, out pageSize);
                }
                var querySpecification = new QuerySpecification("SELECT * FROM enrollmentGroups");
                using var registrationQuery = _client.CreateEnrollmentGroupQuery(
                    querySpecification, pageSize ?? 0, ct);

                registrationQuery.ContinuationToken = continuation;
                var result = await registrationQuery.NextAsync().ConfigureAwait(false);
                return new EnrollmentGroupListModel {
                    ContinuationToken = _serializer.SerializeContinuationToken(query,
                        result.ContinuationToken, pageSize),
                    Groups = result.Items
                        .OfType<EnrollmentGroup>()
                        .Select(s => s.ToEnrollmentGroupModel(_serializer))
                };
            }
            catch (ProvisioningServiceClientException e) {
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceRegistrationListModel> QueryEnrollmentGroupRegistrationsAsync(
            string enrollmentGroupId, string query, string continuation, int? pageSize, CancellationToken ct) {
            try {
                if (continuation != null) {
                    _serializer.DeserializeContinuationToken(continuation,
                        out query, out continuation, out pageSize);
                }
                var querySpecification = new QuerySpecification("SELECT * FROM enrollmentGroups");
                using var registrationQuery = _client.CreateEnrollmentGroupRegistrationStateQuery(
                    querySpecification, enrollmentGroupId, pageSize ?? 0, ct);

                registrationQuery.ContinuationToken = continuation;
                var result = await registrationQuery.NextAsync().ConfigureAwait(false);
                return new DeviceRegistrationListModel {
                    ContinuationToken = _serializer.SerializeContinuationToken(query,
                        result.ContinuationToken, pageSize),
                    Registrations = result.Items
                        .OfType<DeviceRegistrationState>()
                        .Select(s => s.ToDeviceRegistrationModel(enrollmentGroupId))
                };
            }
            catch (ProvisioningServiceClientException e) {
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<EnrollmentGroupModel> CreateEnrollmentGroupAsync(string enrollmentGroupId,
            AuthenticationModel attestation, DeviceRegistrationModel registration, CancellationToken ct) {
            if (string.IsNullOrEmpty(enrollmentGroupId)) {
                throw new ArgumentNullException(nameof(enrollmentGroupId));
            }
            if (attestation == null) {
                throw new ArgumentNullException(nameof(attestation));
            }
            var keys = new SymmetricKeyAttestation(attestation.PrimaryKey, attestation.SecondaryKey);
            var enrollmentGroup = registration.ToEnrollmentGroup(enrollmentGroupId, keys);
            try {
                var group = await _client.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup,
                    ct).ConfigureAwait(false);
                return group.ToEnrollmentGroupModel(_serializer);
            }
            catch (ProvisioningServiceClientException e) {
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<EnrollmentGroupModel> CreateEnrollmentGroupAsync(string enrollmentGroupId,
            X509Certificate2 certificate, DeviceRegistrationModel registration, CancellationToken ct) {
            if (string.IsNullOrEmpty(enrollmentGroupId)) {
                throw new ArgumentNullException(nameof(enrollmentGroupId));
            }
            if (certificate == null) {
                throw new ArgumentNullException(nameof(certificate));
            }
            var attestation = X509Attestation.CreateFromClientCertificates(certificate);
            var enrollmentGroup = registration.ToEnrollmentGroup(enrollmentGroupId, attestation);
            try {
                var group = await _client.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup,
                    ct).ConfigureAwait(false);
                return group.ToEnrollmentGroupModel(_serializer);
            }
            catch (ProvisioningServiceClientException e) {
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task<EnrollmentGroupModel> GetEnrollmentGroupAsync(string enrollmentGroupId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(enrollmentGroupId)) {
                throw new ArgumentNullException(nameof(enrollmentGroupId));
            }
            try {
                var group = await _client.GetEnrollmentGroupAsync(enrollmentGroupId, ct).ConfigureAwait(false);
                return group.ToEnrollmentGroupModel(_serializer);
            }
            catch (ProvisioningServiceClientException e) {
                throw e.Translate();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteEnrollmentGroupAsync(string enrollmentGroupId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(enrollmentGroupId)) {
                throw new ArgumentNullException(nameof(enrollmentGroupId));
            }
            try {
                await _client.DeleteEnrollmentGroupAsync(enrollmentGroupId, ct).ConfigureAwait(false);
            }
            catch (Exception e) {
                throw e.Translate();
            }
        }

        private readonly ServiceClient _client;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
    }
}
