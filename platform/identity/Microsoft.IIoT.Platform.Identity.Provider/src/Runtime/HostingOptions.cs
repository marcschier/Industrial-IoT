// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Provider.Runtime {
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Common web service configuration aggregation
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<WebHostOptions> {

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, WebHostOptions options) {
            options.ServicePathBase = GetStringOrDefault(
                PcsVariable.PCS_AUTH_SERVICE_PATH_BASE);
        }
    }
}
