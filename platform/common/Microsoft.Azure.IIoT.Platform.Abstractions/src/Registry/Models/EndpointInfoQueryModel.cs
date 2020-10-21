// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Endpoint query
    /// </summary>
    public class EndpointInfoQueryModel {

        /// <summary>
        /// Endoint url for direct server access
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Certificate thumbprint of the endpoint
        /// </summary>
        public string Certificate { get; set; }

        /// <summary>
        /// Endpoint security policy to use - null = Best.
        /// </summary>
        public string SecurityPolicy { get; set; }

        /// <summary>
        /// Security mode to use for communication - null = Best
        /// </summary>
        public SecurityMode? SecurityMode { get; set; }

        /// <summary>
        /// Whether to test for visibility
        /// </summary>
        public EntityVisibility? Visibility { get; set; }

        /// <summary>
        /// Discoverer id to filter with
        /// </summary>
        public string DiscovererId { get; set; }

        /// <summary>
        /// Application id to filter
        /// </summary>
        public string ApplicationId { get; set; }
    }
}

