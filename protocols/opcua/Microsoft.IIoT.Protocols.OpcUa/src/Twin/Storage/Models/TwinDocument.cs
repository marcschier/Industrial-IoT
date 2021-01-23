// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Storage.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Twin document persisted and comparable
    /// </summary>
    [DataContract]
    public sealed class TwinDocument {

        /// <summary>
        /// Twin id
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Class type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = IdentityType.Twin;

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        [DataMember]
        public string EndpointId { get; set; }

        /// <summary>
        /// Connection status
        /// </summary>
        [DataMember]
        public ConnectionStatus ConnectionState { get; set; }

        /// <summary>
        /// Create time
        /// </summary>
        [DataMember]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        [DataMember]
        public string CreateAuthorityId { get; set; }

        /// <summary>
        /// Update time
        /// </summary>
        [DataMember]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        [DataMember]
        public string UpdateAuthorityId { get; set; }

        /// <summary>
        /// Operation timeout
        /// </summary>
        [DataMember]
        public TimeSpan? OperationTimeout { get; internal set; }

        /// <summary>
        /// Audit id to use
        /// </summary>
        [DataMember]
        public string DiagnosticsAuditId { get; internal set; }

        /// <summary>
        /// Diagnostic level
        /// </summary>
        [DataMember]
        public DiagnosticsLevel? DiagnosticsLevel { get; internal set; }

        /// <summary>
        /// Last result diagnostic info
        /// </summary>
        [DataMember]
        public VariantValue LastResultDiagnostics { get; internal set; }

        /// <summary>
        /// Last Error message
        /// </summary>
        [DataMember]
        public string LastResultErrorMessage { get; internal set; }

        /// <summary>
        /// last result status
        /// </summary>
        [DataMember]
        public uint? LastResultStatusCode { get; internal set; }

        /// <summary>
        /// Last result change time
        /// </summary>
        [DataMember]
        public DateTime? LastResultChange { get; internal set; }

        /// <summary>
        /// Credential value
        /// </summary>
        [DataMember]
        public VariantValue Credential { get; internal set; }

        /// <summary>
        /// Credential type
        /// </summary>
        [DataMember]
        public CredentialType CredentialType { get; internal set; }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (obj is not TwinDocument document) {
                return false;
            }
            if (Id != document.Id) {
                return false;
            }
            if (ClassType != document.ClassType) {
                return false;
            }
            if (EndpointId != document.EndpointId) {
                return false;
            }
            if (UpdateTime != document.UpdateTime) {
                return false;
            }
            if (UpdateAuthorityId != document.UpdateAuthorityId) {
                return false;
            }
            if (CreateAuthorityId != document.CreateAuthorityId) {
                return false;
            }
            if (CreateTime != document.CreateTime) {
                return false;
            }
            if (OperationTimeout != document.OperationTimeout) {
                return false;
            }
            if (CredentialType != document.CredentialType) {
                return false;
            }
            if (Credential != document.Credential) {
                return false;
            }
            if (LastResultChange != document.LastResultChange) {
                return false;
            }
            if (LastResultDiagnostics != document.LastResultDiagnostics) {
                return false;
            }
            if (LastResultStatusCode != document.LastResultStatusCode) {
                return false;
            }
            if (LastResultErrorMessage != document.LastResultErrorMessage) {
                return false;
            }
            if (DiagnosticsAuditId != document.DiagnosticsAuditId) {
                return false;
            }
            if (DiagnosticsLevel != document.DiagnosticsLevel) {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public static bool operator ==(TwinDocument r1,
            TwinDocument r2) =>
            EqualityComparer<TwinDocument>.Default.Equals(r1, r2);
        /// <inheritdoc/>
        public static bool operator !=(TwinDocument r1,
            TwinDocument r2) =>
            !(r1 == r2);

        /// <inheritdoc/>
        public override int GetHashCode() {
            var hash = new System.HashCode();
            hash.Add(Id);
            hash.Add(ClassType);
            hash.Add(EndpointId);
            hash.Add(UpdateTime);
            hash.Add(UpdateAuthorityId);
            hash.Add(CreateTime);
            hash.Add(CreateAuthorityId);
            hash.Add(OperationTimeout);
            hash.Add(CredentialType);
            hash.Add(Credential);
            hash.Add(LastResultChange);
            hash.Add(LastResultStatusCode);
            hash.Add(LastResultErrorMessage);
            hash.Add(LastResultDiagnostics);
            hash.Add(DiagnosticsAuditId);
            hash.Add(DiagnosticsLevel);
            return hash.ToHashCode();
        }
    }
}
