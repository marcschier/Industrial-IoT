// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.IIoT.Serializers;

    /// <summary>
    /// Model of device registry / twin document
    /// </summary>
    public class DeviceTwinModel {

        /// <summary>
        /// Device id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Module id
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Etag for comparison
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Tags
        /// </summary>
        public Dictionary<string, VariantValue> Tags { get; set; }

        /// <summary>
        /// Settings
        /// </summary>
        public TwinPropertiesModel Properties { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        public DeviceCapabilitiesModel Capabilities { get; set; }

        /// <summary>
        /// Twin's Version
        /// </summary>
        public long? Version { get; set; }

        /// <summary>
        /// Gets or sets the corresponding Device's Status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Reason, if any, for the corresponding Device
        /// to be in specified <see cref="Status"/>
        /// </summary>
        public string StatusReason { get; set; }

        /// <summary>
        /// Time when the corresponding Device's
        /// <see cref="Status"/> was last updated
        /// </summary>
        public DateTimeOffset? StatusUpdatedTime { get; set; }

        /// <summary>
        /// Corresponding Device's ConnectionState
        /// </summary>
        public string ConnectionState { get; set; }

        /// <summary>
        /// Time when the corresponding Device was last active
        /// </summary>
        public DateTimeOffset? LastActivityTime { get; set; }
    }
}
