// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.DependencyInjection {
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime;
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

            // No services associated

            services.AddOptions();
            services.AddTransient<IPostConfigureOptions<HeadersOptions>, HeadersConfig>();
            services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, HeadersConfig>();
            services.AddTransient<IConfigureNamedOptions<ForwardedHeadersOptions>, HeadersConfig>();
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
            services.AddHttpsRedirection(options => { });

            services.AddOptions();
            services.AddTransient<IPostConfigureOptions<WebHostOptions>, WebHostConfig>();
            services.AddTransient<IConfigureOptions<HttpsRedirectionOptions>, WebHostConfig>();
            services.AddTransient<IConfigureNamedOptions<HttpsRedirectionOptions>, WebHostConfig>();
            return services;
        }
    }
}
