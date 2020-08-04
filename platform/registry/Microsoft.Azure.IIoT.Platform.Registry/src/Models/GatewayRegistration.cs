// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Hub;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Edge gateway registration
    /// </summary>
    [DataContract]
    public sealed class GatewayRegistration : EntityRegistration {

        /// <inheritdoc/>
        [DataMember]
        public override string DeviceType => IdentityType.Gateway;

        /// <summary>
        /// Site of the registration
        /// </summary>
        [DataMember]
        public string SiteId { get; set; }

        /// <summary>
        /// Searchable grouping (either device or site id)
        /// </summary>
        [DataMember]
        public string SiteOrGatewayId =>
            !string.IsNullOrEmpty(SiteId) ? SiteId : DeviceId;

        /// <summary>
        /// Create registration - for testing purposes
        /// </summary>
        /// <param name="deviceId"></param>
        public GatewayRegistration(string deviceId = null) {
            DeviceId = deviceId;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is GatewayRegistration registration)) {
                return false;
            }
            if (!base.Equals(registration)) {
                return false;
            }
            if (SiteId != registration.SiteId) {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public static bool operator ==(GatewayRegistration r1,
            GatewayRegistration r2) => EqualityComparer<GatewayRegistration>.Default.Equals(r1, r2);

        /// <inheritdoc/>
        public static bool operator !=(GatewayRegistration r1,
            GatewayRegistration r2) => !(r1 == r2);

        /// <inheritdoc/>
        public override int GetHashCode() {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * -1521134295) +
                EqualityComparer<string>.Default.GetHashCode(SiteId);
            return hashCode;
        }

        internal bool IsInSync() {
            return _isInSync;
        }

        internal bool IsConnected() {
            return Connected;
        }

        internal bool _isInSync;
    }
}
