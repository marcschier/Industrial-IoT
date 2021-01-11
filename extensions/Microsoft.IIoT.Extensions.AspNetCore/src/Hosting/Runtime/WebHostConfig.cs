// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Hosting.Runtime {
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpsPolicy;

    /// <summary>
    /// Web Host configuration
    /// </summary>
    internal sealed class WebHostConfig : PostConfigureOptionBase<WebHostOptions>,
        IConfigureOptions<HttpsRedirectionOptions>,
        IConfigureNamedOptions<HttpsRedirectionOptions> {

        /// <inheritdoc/>
        public WebHostConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, WebHostOptions options) {
            options.UseHttpsRedirect = GetIntOrNull(
                AspNetCoreVariable.ASPNETCORE_HTTPSREDIRECTPORT) != null;
        }

        /// <inheritdoc/>
        public void Configure(string name, HttpsRedirectionOptions options) {
            options.HttpsPort = GetIntOrNull(
                AspNetCoreVariable.ASPNETCORE_HTTPSREDIRECTPORT, options.HttpsPort);
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        }

        /// <inheritdoc/>
        public void Configure(HttpsRedirectionOptions options) {
            Configure(Options.DefaultName, options);
        }
    }
}
