// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Runtime {
    using Microsoft.Azure.IIoT.Authentication.Runtime;
    using Microsoft.Azure.IIoT.Api.Runtime;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.AppInsights.Runtime;
    using Microsoft.Azure.IIoT.Azure.SignalR;
    using Microsoft.Azure.IIoT.Azure.SignalR.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration aggregation
    /// </summary>
    public class Config : ApiConfig, IAppInsightsConfig, ISignalRServiceConfig,
        IWebHostConfig, IHeadersConfig {

        /// <summary>Url</summary>
        public string TsiDataAccessFQDN =>
            GetStringOrDefault(PcsVariable.PCS_TSI_URL)?.Trim();

        /// <summary>TenantId</summary>
        public string TenantId =>
            GetStringOrDefault(PcsVariable.PCS_AUTH_TENANT)?.Trim();

        /// <summary>WorkbookId</summary>
        public string WorkbookId =>
            GetStringOrDefault(PcsVariable.PCS_WORKBOOK_ID)?.Trim();

        /// <summary>SubscriptionId</summary>
        public string SubscriptionId =>
            GetStringOrDefault(PcsVariable.PCS_SUBSCRIPTION_ID)?.Trim();

        /// <summary>ResourceGroup Name</summary>
        public string ResourceGroup =>
            GetStringOrDefault(PcsVariable.PCS_RESOURCE_GROUP)?.Trim();

        /// <inheritdoc/>
        public string SignalRConnString => _sr.SignalRConnString;
        /// <inheritdoc/>
        public bool SignalRServerLess => _sr.SignalRServerLess;

        /// <inheritdoc/>
        public int HttpsRedirectPort => _host.HttpsRedirectPort;
        /// <inheritdoc/>
        public string ServicePathBase => GetStringOrDefault(
            PcsVariable.PCS_FRONTEND_APP_SERVICE_PATH_BASE,
                () => _host.ServicePathBase);

        /// <inheritdoc/>
        public bool AspNetCoreForwardedHeadersEnabled =>
            _fh.AspNetCoreForwardedHeadersEnabled;
        /// <inheritdoc/>
        public int AspNetCoreForwardedHeadersForwardLimit =>
            _fh.AspNetCoreForwardedHeadersForwardLimit;

        /// <inheritdoc/>
        public string InstrumentationKey => _ai.InstrumentationKey;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration) :
            base(configuration) {

            _host = new WebHostConfig(configuration);
            _fh = new HeadersConfig(configuration);
            _sr = new SignalRServiceConfig(configuration);
            _ai = new AppInsightsConfig(configuration);
        }

        private readonly AppInsightsConfig _ai;
        private readonly SignalRServiceConfig _sr;
        private readonly WebHostConfig _host;
        private readonly HeadersConfig _fh;
    }
}
