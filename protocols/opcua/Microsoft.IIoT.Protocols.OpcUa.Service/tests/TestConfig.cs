// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Protocols.OpcUa.Api;
    using Microsoft.IIoT.Extensions.Configuration;
    using System;
    using Microsoft.Extensions.Options;

    /// <inheritdoc/>
    public class TestConfig : PostConfigureOptionBase<OpcUaApiOptions> {

        /// <inheritdoc/>
        public TestConfig(Uri baseAddress) {
            _url = baseAddress?.ToString().TrimEnd('/');
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, OpcUaApiOptions options) {
            options.OpcUaServiceUrl = _url;
        }

        /// <summary>
        /// Get as options
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        public static IOptions<OpcUaApiOptions> ToOptions(Uri baseAddress) {
            return new TestConfig(baseAddress).ToOptions();
        }

        private readonly string _url;
    }
}
