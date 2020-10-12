// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hub;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Application document persisted and comparable
    /// </summary>
    [DataContract]
    public sealed class ApplicationDocument {

        /// <summary>
        /// Application document id
        /// </summary>
        [DataMember(Name = "id")]
        public string Id => ApplicationInfoModelEx.CreateApplicationId(
             DiscovererId, ApplicationUri, ApplicationType);

        /// <summary>
        /// Class type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = IdentityType.Application;

        /// <summary>
        /// Whether document is enabled or not
        /// </summary>
        [DataMember]
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// Last time application was seen
        /// </summary>
        [DataMember]
        public DateTime? NotSeenSince { get; set; }

        /// <summary>
        /// Identity that owns the twin.
        /// </summary>
        [DataMember]
        public string DiscovererId { get; set; }

        /// <summary>
        /// Application uri
        /// </summary>
        [DataMember]
        public string ApplicationUri { get; set; }

        /// <summary>
        /// Upper case application url
        /// </summary>
        [DataMember]
        public string ApplicationUriUC => ApplicationUri?.ToUpperInvariant();

        /// <summary>
        /// Application name
        /// </summary>
        [DataMember]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Application name locale
        /// </summary>
        [DataMember]
        public string Locale { get; set; }

        /// <summary>
        /// Application name locale
        /// </summary>
        [DataMember]
        public /*IReadOnlyDictionary*/ IDictionary<string, string> LocalizedNames { get; set; }

        /// <summary>
        /// Discovery profile uri
        /// </summary>
        [DataMember]
        public string DiscoveryProfileUri { get; set; }

        /// <summary>
        /// Gateway server uri
        /// </summary>
        [DataMember]
        public string GatewayServerUri { get; set; }

        /// <summary>
        /// Product uri
        /// </summary>
        [DataMember]
        public string ProductUri { get; set; }

        /// <summary>
        /// Application type
        /// </summary>
        [DataMember]
        public ApplicationType? ApplicationType { get; set; }

        /// <summary>
        /// Returns discovery urls of the application
        /// </summary>
        [DataMember]
        public /*ReadOnlySet*/ ISet<string> DiscoveryUrls { get; set; }

        /// <summary>
        /// Host address of server application
        /// </summary>
        [DataMember]
        public /*ReadOnlySet*/ ISet<string> HostAddresses { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        [DataMember]
        public /*ReadOnlySet*/ ISet<string> Capabilities { get; set; }

        /// <summary>
        /// Create time
        /// </summary>
        [DataMember]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        [DataMember]
        public string CreateAuthorityId { get; set; }

        /// <summary>
        /// Update time
        /// </summary>
        [DataMember]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        [DataMember]
        public string UpdateAuthorityId { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (obj is not ApplicationDocument document) {
                return false;
            }
            if (Id != document.Id) {
                return false;
            }
            if (ClassType != document.ClassType) {
                return false;
            }
            if ((IsDisabled ?? false) != (document.IsDisabled ?? false)) {
                return false;
            }
            if (NotSeenSince != document.NotSeenSince) {
                return false;
            }
            if (DiscovererId != document.DiscovererId) {
                return false;
            }
            if (ApplicationType != document.ApplicationType) {
                return false;
            }
            if (ApplicationUriUC != document.ApplicationUriUC) {
                return false;
            }
            if (DiscoveryProfileUri != document.DiscoveryProfileUri) {
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
            if (GatewayServerUri != document.GatewayServerUri) {
                return false;
            }
            if (ProductUri != document.ProductUri) {
                return false;
            }
            if (!HostAddresses.SetEqualsSafe(document.HostAddresses)) {
                return false;
            }
            if (ApplicationName != document.ApplicationName) {
                return false;
            }
            if (!LocalizedNames.DictionaryEqualsSafe(
                document.LocalizedNames)) {
                return false;
            }
            if (!Capabilities.SetEqualsSafe(document.Capabilities)) {
                return false;
            }
            if (!DiscoveryUrls.SetEqualsSafe(document.DiscoveryUrls)) {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public static bool operator ==(ApplicationDocument r1,
            ApplicationDocument r2) =>
            EqualityComparer<ApplicationDocument>.Default.Equals(r1, r2);
        /// <inheritdoc/>
        public static bool operator !=(ApplicationDocument r1,
            ApplicationDocument r2) => !(r1 == r2);

        /// <inheritdoc/>
        public override int GetHashCode() {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Id);
            hash.Add(ClassType);
            hash.Add(NotSeenSince);
            hash.Add(IsDisabled);
            hash.Add(DiscovererId);
            hash.Add(ApplicationType);
            hash.Add(ProductUri);
            hash.Add(DiscoveryProfileUri);
            hash.Add(GatewayServerUri);
            hash.Add(ApplicationName);
            return hash.ToHashCode();
        }
    }
}
