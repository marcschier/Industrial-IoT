// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service {
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Web host configuration
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<WebHostOptions> {

        /// <inheritdoc/>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, WebHostOptions options) {
            options.ServicePathBase = GetStringOrDefault(
                PcsVariable.PCS_TWIN_SERVICE_PATH_BASE);
        }
    }
}
