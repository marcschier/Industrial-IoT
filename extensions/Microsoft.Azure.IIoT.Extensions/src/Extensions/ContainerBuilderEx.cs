// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Logging.Debug;
    using Autofac.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Container builder extensions
    /// </summary>
    public static class ContainerBuilderExtensions {

        /// <summary>
        /// Add services to container
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder ConfigureServices(this ContainerBuilder builder,
            Action<IServiceCollection> configure) {
            var services = new ServiceCollection();
            configure(services);
            builder.Populate(services);
            return builder;
        }

        /// <summary>
        /// Configure options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder Configure<TOptions>(this ContainerBuilder builder,
            Action<TOptions> configure) where TOptions : class {
            builder.RegisterInstance(new ConfigureOptions<TOptions>(configure))
                .AsImplementedInterfaces();
            return builder;
        }

        /// <summary>
        /// Add options to container
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder AddOptions(this ContainerBuilder builder) {
            builder.RegisterGeneric(typeof(OptionsManager<>))
                .SingleInstance()
                .As(typeof(IOptions<>));
            builder.RegisterGeneric(typeof(OptionsManager<>))
                .InstancePerLifetimeScope()
                .As(typeof(IOptionsSnapshot<>));
            builder.RegisterGeneric(typeof(OptionsMonitor<>))
                .SingleInstance()
                .As(typeof(IOptionsMonitor<>));
            builder.RegisterGeneric(typeof(OptionsFactory<>))
                .InstancePerDependency()
                .As(typeof(IOptionsFactory<>));
            builder.RegisterGeneric(typeof(OptionsCache<>))
                .SingleInstance()
                .As(typeof(IOptionsMonitorCache<>));
            return builder;
        }

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
            builder.RegisterModule<Logging>();

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
