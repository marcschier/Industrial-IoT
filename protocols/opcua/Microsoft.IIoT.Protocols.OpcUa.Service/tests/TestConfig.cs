// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
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
    }
}
