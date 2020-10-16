// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Hub;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Endpoint document persisted and comparable
    /// </summary>
    [DataContract]
    public sealed class EndpointDocument {

        /// <summary>
        /// Endpoint id
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Class type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = IdentityType.Endpoint;

        /// <summary>
        /// Last time endpoint was seen
        /// </summary>
        [DataMember]
        public DateTime? NotSeenSince { get; set; }

        /// <summary>
        /// Endpoint visibility
        /// </summary>
        [DataMember]
        public EntityVisibility Visibility { get; set; }

        /// <summary>
        /// Identity that owns the twin.
        /// </summary>
        [DataMember]
        public string DiscovererId { get; set; }

        /// <summary>
        /// Application id of twin
        /// </summary>
        [DataMember]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Security level of endpoint
        /// </summary>
        [DataMember]
        public int? SecurityLevel { get; set; }

        /// <summary>
        /// The credential policies supported by the registered endpoint
        /// </summary>
        [DataMember]
        public /*IReadOnlyList*/ IList<AuthenticationMethodModel> AuthenticationMethods { get; set; }

        /// <summary>
        /// Endoint url for direct server access
        /// </summary>
        [DataMember]
        public string EndpointUrl { get; set; }

        /// <summary>
        /// Alternative urls
        /// </summary>
        [DataMember]
        public /*IReadOnlySet*/ ISet<string> AlternativeUrls { get; set; }

        /// <summary>
        /// Endpoint security policy to use.
        /// </summary>
        [DataMember]
        public string SecurityPolicy { get; set; }

        /// <summary>
        /// Security mode to use for communication
        /// </summary>
        [DataMember]
        public SecurityMode SecurityMode { get; set; }

        /// <summary>
        /// Certificate Thumbprint
        /// </summary>
        [DataMember]
        public string Thumbprint { get; set; }

        /// <summary>
        /// Activation state
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public EntityActivationState ActivationState { get; set; }

        /// <summary>
        /// Endpoint connectivity status
        /// </summary>
        [DataMember]
        public EndpointConnectivityState EndpointState { get; set; }

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

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (obj is not EndpointDocument document) {
                return false;
            }
            if (Id != document.Id) {
                return false;
            }
            if (ClassType != document.ClassType) {
                return false;
            }
            if (NotSeenSince != document.NotSeenSince) {
                return false;
            }
            if (ActivationState != document.ActivationState) {
                return false;
            }
            if (EndpointState != document.EndpointState) {
                return false;
            }
            if (DiscovererId != document.DiscovererId) {
                return false;
            }
            if (ApplicationId != document.ApplicationId) {
                return false;
            }
            if (EndpointUrl != document.EndpointUrl) {
                return false;
            }
            if (SecurityLevel != document.SecurityLevel) {
                return false;
            }
            if (SecurityPolicy != document.SecurityPolicy) {
                return false;
            }
            if (SecurityMode != document.SecurityMode) {
                return false;
            }
            if (Thumbprint != document.Thumbprint) {
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
            return true;
        }

        /// <inheritdoc/>
        public static bool operator ==(EndpointDocument r1,
            EndpointDocument r2) =>
            EqualityComparer<EndpointDocument>.Default.Equals(r1, r2);
        /// <inheritdoc/>
        public static bool operator !=(EndpointDocument r1,
            EndpointDocument r2) =>
            !(r1 == r2);

        /// <inheritdoc/>
        public override int GetHashCode() {
            var hash = new System.HashCode();
            hash.Add(Id);
            hash.Add(ClassType);
            hash.Add(NotSeenSince);
            hash.Add(EndpointState);
            hash.Add(ActivationState);
            hash.Add(EndpointUrl);
            hash.Add(DiscovererId);
            hash.Add(ApplicationId);
            hash.Add(Thumbprint);
            hash.Add(SecurityLevel);
            hash.Add(SecurityMode);
            hash.Add(SecurityPolicy);
            hash.Add(UpdateTime);
            hash.Add(UpdateAuthorityId);
            hash.Add(CreateTime);
            hash.Add(CreateAuthorityId);
            return hash.ToHashCode();
        }
    }
}
