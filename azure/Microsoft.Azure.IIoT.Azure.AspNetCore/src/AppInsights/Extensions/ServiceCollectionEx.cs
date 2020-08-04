// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.AppInsights {
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Configure OpenApi
    /// </summary>
    public static class ServiceCollectionEx {

        /// <summary>
        /// Configure AppInsights
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppInsightsTelemetry(this IServiceCollection services) {

            services.AddApplicationInsightsTelemetry();
            services.AddTransient<IConfigureOptions<ApplicationInsightsServiceOptions>>(provider => {
                var config = provider.GetRequiredService<IAppInsightsConfig>();
                return new ConfigureNamedOptions<ApplicationInsightsServiceOptions>(Options.DefaultName, options => {
                    options.InstrumentationKey = config.InstrumentationKey;
                });
            });
            services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
            return services;
        }
    }
}

