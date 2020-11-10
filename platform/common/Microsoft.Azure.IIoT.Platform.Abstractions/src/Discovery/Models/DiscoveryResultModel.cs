// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Models {
    using System;

    /// <summary>
    /// One of n items with the discovered application info
    /// </summary>
    public class DiscoveryResultModel {

        /// <summary>
        /// Timestamp of the discovery sweep.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Index in the batch with same timestamp.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Discovered endpoint in form of endpoint registration
        /// </summary>
        public EndpointInfoModel Endpoint { get; set; }

        /// <summary>
        /// Application to which this endpoint belongs
        /// </summary>
        public ApplicationInfoModel Application { get; set; }

        /// <summary>
        /// Discovery result summary on last element
        /// </summary>
        public DiscoveryContextModel Result { get; set; }
    }
}