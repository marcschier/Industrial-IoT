// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.OpenApi.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// OpenApi configuration 
    /// </summary>
    internal class OpenApiConfig : PostConfigureOptionBase<OpenApiOptions> {

        /// <inheritdoc/>
        public OpenApiConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, OpenApiOptions options) {
            if (string.IsNullOrEmpty(options.OpenApiAuthorizationEndpoint)) {
                var tenantId = GetStringOrDefault(PcsVariable.PCS_AUTH_TENANT,
                    "common")?.Trim();
                var instanceUrl = GetStringOrDefault(PcsVariable.PCS_AAD_INSTANCE,
                    "https://login.microsoftonline.com")?.Trim();

                options.OpenApiAuthorizationEndpoint =
                    $"{instanceUrl.TrimEnd('/')}/{tenantId}/oauth2/authorize";
            }
            if (string.IsNullOrEmpty(options.OpenApiAppId)) {
                options.OpenApiAppId = GetStringOrDefault(PcsVariable.PCS_OPENAPI_APPID,
                    GetStringOrDefault(PcsVariable.PCS_AAD_CONFIDENTIAL_CLIENT_APPID,
                    GetStringOrDefault("PCS_WEBUI_AUTH_AAD_APPID")))?.Trim();
                options.OpenApiAppSecret = GetStringOrDefault(PcsVariable.PCS_OPENAPI_APP_SECRET,
                    GetStringOrDefault(PcsVariable.PCS_AAD_CONFIDENTIAL_CLIENT_SECRET,
                    GetStringOrDefault("PCS_APPLICATION_SECRET")))?.Trim();

                // Disable if no appid
                options.WithAuth = GetBoolOrDefault(PcsVariable.PCS_AUTH_REQUIRED,
                    !string.IsNullOrEmpty(options.OpenApiAppId));
                // Disable with auth but no appid
                options.UIEnabled = GetBoolOrDefault(PcsVariable.PCS_OPENAPI_ENABLED,
                    !options.WithAuth || !string.IsNullOrEmpty(options.OpenApiAppId));
            }

            if (string.IsNullOrEmpty(options.OpenApiServerHost)) {
                options.OpenApiServerHost =
                GetStringOrDefault(PcsVariable.PCS_OPENAPI_SERVER_HOST)?.Trim();
            }

            if (options.Version != 2 && options.Version != 3) {
                var useV2 = GetBoolOrDefault(PcsVariable.PCS_OPENAPI_USE_V2,
                    GetBoolOrDefault("PCS_SWAGGER_V2", true));
                options.Version = useV2 ? 2 : 3;
            }
        }
    }
}
