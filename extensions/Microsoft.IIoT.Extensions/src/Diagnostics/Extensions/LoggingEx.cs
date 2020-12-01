// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Autofac.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Container builder Logging extensions
    /// </summary>
    public static class LoggingEx {

        /// <summary>
        /// Register default diagnostics
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder AddDiagnostics(this ContainerBuilder builder,
            Action<ILoggingBuilder> configure = null) {
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterType<HealthCheckRegistrar>()
                .AsImplementedInterfaces().SingleInstance();

            // Add logging
            builder.AddOptions();
            builder.RegisterModule<Logging>();

            var log = new LogBuilder();
            if (configure != null) {
                configure(log);
            }
            else {
                log.AddConsole();
                log.AddDebug();
            }

            builder.Populate(log.Services);
            return builder;
        }

        /// <summary>
        /// Log builder adapter
        /// </summary>
        private class LogBuilder : ILoggingBuilder {

            /// <inheritdoc/>
            public IServiceCollection Services { get; }
                = new ServiceCollection();
        }
    }
}
