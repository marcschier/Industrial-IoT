// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Service {
    using Microsoft.IIoT.Platform.Publisher.Api;
    using System;

    /// <inheritdoc/>
    public class TestConfig : IPublisherConfig {

        /// <summary>
        /// Create test configuration
        /// </summary>
        /// <param name="baseAddress"></param>
        public TestConfig(Uri baseAddress) {
            OpcUaPublisherServiceUrl = baseAddress.ToString().TrimEnd('/');
        }

        /// <inheritdoc/>
        public string OpcUaPublisherServiceUrl { get; }
    }
}
