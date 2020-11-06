// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.HttpOverrides;

    /// <summary>
    /// Forwarded headers processing configuration.
    /// </summary>
    internal sealed class HeadersConfig : PostConfigureOptionBase<HeadersOptions>,
        IConfigureOptions<ForwardedHeadersOptions>, 
        IConfigureNamedOptions<ForwardedHeadersOptions> {

        /// <inheritdoc/>
        public HeadersConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, HeadersOptions options) {
            if (!options.ForwardingEnabled) {
                options.ForwardingEnabled = GetBoolOrDefault(
                    AspNetCoreVariable.ASPNETCORE_FORWARDEDHEADERS_ENABLED);
            }
        }

        /// <inheritdoc/>
        public void Configure(string name, ForwardedHeadersOptions options) {
            options.ForwardLimit = GetIntOrNull(
                AspNetCoreVariable.ASPNETCORE_FORWARDEDHEADERS_FORWARDLIMIT,
                    options.ForwardLimit);
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
            // Only loopback proxies are allowed by default.
            // Clear that restriction because forwarders are enabled by explicit
            // configuration.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        }

        /// <inheritdoc/>
        public void Configure(ForwardedHeadersOptions options) {
            Configure(Options.DefaultName, options);
        }
    }
}
