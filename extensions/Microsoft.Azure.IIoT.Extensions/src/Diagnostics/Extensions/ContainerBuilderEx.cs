// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Logging.Debug;
    using System;

    /// <summary>
    /// Register debug logger
    /// </summary>
    public static class ContainerBuilderEx {

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
            builder.RegisterType<DebugLoggerProvider>()
                .AsImplementedInterfaces();
            builder.RegisterType<ConsoleLoggerProvider>()
                .AsImplementedInterfaces();
            builder.RegisterModule<Log>();

            var log = new LogBuilder();
            if (configure != null) {
                configure(log);
            }
            else {
                log.AddConsole();
                log.AddDebug();
            }
            log.Services.AddOptions();
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
