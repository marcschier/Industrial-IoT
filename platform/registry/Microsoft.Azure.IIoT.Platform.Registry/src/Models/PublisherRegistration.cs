// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Hub;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Publisher agent module registration
    /// </summary>
    [DataContract]
    public sealed class PublisherRegistration : EntityRegistration {

        /// <inheritdoc/>
        [DataMember]
        public override string DeviceType => IdentityType.Publisher;

        /// <summary>
        /// Device id for registration
        /// </summary>
        [DataMember]
        public string ModuleId { get; set; }

        /// <summary>
        /// Current log level
        /// </summary>
        [DataMember]
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Create registration - for testing purposes
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        public PublisherRegistration(string deviceId = null,
            string moduleId = null) {
            DeviceId = deviceId;
            ModuleId = moduleId;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (obj is not PublisherRegistration registration) {
                return false;
            }
            if (!base.Equals(registration)) {
                return false;
            }
            if (ModuleId != registration.ModuleId) {
                return false;
            }
            if (LogLevel != registration.LogLevel) {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public static bool operator ==(PublisherRegistration r1,
            PublisherRegistration r2) => EqualityComparer<PublisherRegistration>.Default.Equals(r1, r2);

        /// <inheritdoc/>
        public static bool operator !=(PublisherRegistration r1,
            PublisherRegistration r2) => !(r1 == r2);

        /// <inheritdoc/>
        public override int GetHashCode() {
            return System.HashCode.Combine(base.GetHashCode(), ModuleId, LogLevel);
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
