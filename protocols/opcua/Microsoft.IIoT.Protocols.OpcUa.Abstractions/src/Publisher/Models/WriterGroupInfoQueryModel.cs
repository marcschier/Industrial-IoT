// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {

    /// <summary>
    /// Writer group registration query request
    /// </summary>
    public class WriterGroupInfoQueryModel {

        /// <summary>
        /// Return groups with this name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// With the specified group version
        /// </summary>
        public uint? GroupVersion { get; set; }

        /// <summary>
        /// Return groups only with this encoding
        /// </summary>
        public NetworkMessageEncoding? Encoding { get; set; }

        /// <summary>
        /// Return groups in the specified state
        /// </summary>
        public WriterGroupStatus? State { get; set; }

        /// <summary>
        /// Return groups only in the specified state
        /// </summary>
        public byte? Priority { get; set; }
    }
}