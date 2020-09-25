// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.DependencyInjection {
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Extension to configure processing of forwarded headers
    /// </summary>
    public static class ServiceCollectionEx {

        /// <summary>
        /// Configure processing of forwarded headers
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddHeaderForwarding(this IServiceCollection services) {
            var fhConfig = services.BuildServiceProvider().GetService<IHeadersConfig>();
            if (fhConfig == null || !fhConfig.AspNetCoreForwardedHeadersEnabled) {
                return services;
            }
            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                if (fhConfig.AspNetCoreForwardedHeadersForwardLimit > 0) {
                    options.ForwardLimit = fhConfig.AspNetCoreForwardedHeadersForwardLimit;
                }

                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            return services;
        }

        /// <summary>
        /// Add https redirection
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddHttpsRedirect(this IServiceCollection services) {
            services.AddHsts(options => {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });
            services.AddHttpsRedirection(options => options.HttpsPort = 0);
            services.AddTransient<IConfigureOptions<HttpsRedirectionOptions>>(services => {
                var config = services.GetService<IWebHostConfig>();
                if (config == null) {
                    throw new InvalidOperationException("Must have configured web host context");
                }
                return new ConfigureNamedOptions<HttpsRedirectionOptions>(
                    Options.DefaultName, options => {
                        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                        options.HttpsPort = config.HttpsRedirectPort;
                    });
            });
            return services;
        }
    }
}
