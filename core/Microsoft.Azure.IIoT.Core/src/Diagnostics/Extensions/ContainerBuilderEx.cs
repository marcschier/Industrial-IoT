// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Logging.Debug;
    using System;

    /// <summary>
    /// Register debug logger
    /// </summary>
    public static class DebugContainerBuilderEx {

        /// <summary>
        /// Register default diagnostics
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        /// <param name="addConsole"></param>
        /// <returns></returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static ContainerBuilder AddDebugDiagnostics(this ContainerBuilder builder,
            IDiagnosticsConfig config = null, bool addConsole = true) {
#pragma warning restore IDE0060 // Remove unused parameter
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterType<HealthCheckRegistrar>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DebugLoggerProvider>()
                .AsImplementedInterfaces();
            builder.RegisterType<ConsoleLoggerProvider>()
                .AsImplementedInterfaces();
            builder.RegisterModule<LoggingModule>();

            return builder;
        }
    }
}
