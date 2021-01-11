// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.LogAnalytics {
    using Microsoft.IIoT.Azure.LogAnalytics.Runtime;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using Autofac;
    using Prometheus;

    /// <summary>
    /// Prometheus module
    /// </summary>
    public class LogAnalyticsMetrics : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<MetricsCollector>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IMetricServer));

            builder.AddOptions();
            builder.RegisterType<LogAnalyticsConfig>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<LogAnalyticsMetricsHandler>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .PropertiesAutowired(
                    PropertyWiringOptions.AllowCircularDependencies);
        }
    }
}
