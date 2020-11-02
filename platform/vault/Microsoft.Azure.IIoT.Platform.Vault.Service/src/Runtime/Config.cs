// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Service.Runtime {
    using Microsoft.Azure.IIoT.Platform.Discovery.Api;
    using Microsoft.Azure.IIoT.Platform.Discovery.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Vault;
    using Microsoft.Azure.IIoT.Platform.Vault.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Cors;
    using Microsoft.Azure.IIoT.AspNetCore.Cors.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Web service configuration
    /// </summary>
    public class Config : ConfigBase, IWebHostConfig,
        ICorsConfig, IOpenApiConfig, IVaultConfig, IRoleConfig,
        IItemContainerConfig, IDiscoveryConfig,
        IHeadersConfig {

        /// <inheritdoc/>
        public bool UseRoles => GetBoolOrDefault(PcsVariable.PCS_AUTH_ROLES);

        /// <inheritdoc/>
        public string CorsWhitelist => _cors.CorsWhitelist;
        /// <inheritdoc/>
        public bool CorsEnabled => _cors.CorsEnabled;

        /// <inheritdoc/>
        public bool UIEnabled => _openApi.UIEnabled;
        /// <inheritdoc/>
        public bool WithAuth => _openApi.WithAuth;
        /// <inheritdoc/>
        public string OpenApiAppId => _openApi.OpenApiAppId;
        /// <inheritdoc/>
        public string OpenApiAppSecret => _openApi.OpenApiAppSecret;
        /// <inheritdoc/>
        public string OpenApiAuthorizationEndpoint => _openApi.OpenApiAuthorizationEndpoint;
        /// <inheritdoc/>
        public bool UseV2 => _openApi.UseV2;
        /// <inheritdoc/>
        public string OpenApiServerHost => _openApi.OpenApiServerHost;

        /// <inheritdoc/>
        public int HttpsRedirectPort => _host.HttpsRedirectPort;
        /// <inheritdoc/>
        public string ServicePathBase => GetStringOrDefault(
            PcsVariable.PCS_VAULT_SERVICE_PATH_BASE,
            () => _host.ServicePathBase);

        /// <inheritdoc/>
        public bool AutoApprove => _vault.AutoApprove;

        /// <inheritdoc/>
        public string ContainerName => "iiot_opc";
        /// <inheritdoc/>
        public string DatabaseName => "iiot_opc";


        /// <inheritdoc/>
        public string OpcUaRegistryServiceUrl => _registry.OpcUaRegistryServiceUrl;

        /// <inheritdoc/>
        public bool AspNetCoreForwardedHeadersEnabled =>
            _fh.AspNetCoreForwardedHeadersEnabled;
        /// <inheritdoc/>
        public int AspNetCoreForwardedHeadersForwardLimit =>
            _fh.AspNetCoreForwardedHeadersForwardLimit;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        internal Config(IConfiguration configuration) :
            base(configuration) {
            _vault = new VaultConfig(configuration);
            _openApi = new OpenApiConfig(configuration);
            _host = new WebHostConfig(configuration);
            _cors = new CorsConfig(configuration);
            _registry = new DiscoveryConfig(configuration);
            _fh = new HeadersConfig(configuration);
        }

        private readonly IVaultConfig _vault;
        private readonly OpenApiConfig _openApi;
        private readonly WebHostConfig _host;
        private readonly CorsConfig _cors;
        private readonly DiscoveryConfig _registry;
        private readonly HeadersConfig _fh;
    }
}

