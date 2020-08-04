// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service {
    using Microsoft.Azure.IIoT.Platform.Twin.Api;
    using System;

    /// <inheritdoc/>
    public class TestConfig : ITwinConfig {

        /// <summary>
        /// Create test configuration
        /// </summary>
        /// <param name="baseAddress"></param>
        public TestConfig(Uri baseAddress) {
            OpcUaTwinServiceUrl = baseAddress.ToString().TrimEnd('/');
        }

        /// <inheritdoc/>
        public string OpcUaTwinServiceUrl { get; }

        /// <inheritdoc/>
        public string OpcUaTwinServiceResourceId => null;
    }
}
