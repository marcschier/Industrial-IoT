// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Debug;
    using Microsoft.Extensions.DependencyInjection;
    using Autofac;
    using Autofac.Core;
    using Autofac.Core.Registration;
    using Autofac.Core.Activators.Reflection;
    using Autofac.Core.Resolving.Pipeline;
    using Autofac.Extensions.DependencyInjection;
    using System;
    using System.Linq;


    /// <summary>
    /// Log module
    /// </summary>
    public class Log : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterGeneric(typeof(Logger<>))
                .SingleInstance()
                .As(typeof(ILogger<>));
            builder.RegisterType(typeof(Logger<Log>)) // Root logger
                .SingleInstance()
                .As(typeof(ILogger));
            builder.RegisterType<LoggerFactory>()
                .As<ILoggerFactory>()
                .SingleInstance()
                .IfNotRegistered(typeof(ILoggerFactory));

            builder.RegisterType<DebugLoggerProvider>()
                .As<ILoggerProvider>()
                .SingleInstance();
            base.Load(builder);
        }

        /// <inheritdoc/>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder registry,
            IComponentRegistration registration) {

            if (registration.Activator is ReflectionActivator ra) {
                try {
                    var ctors = ra.ConstructorFinder.FindConstructors(ra.LimitType);
                    // Only inject logger in components with a ILogger dependency
                    var usesLogger = ctors
                        .SelectMany(ctor => ctor.GetParameters())
                        .Any(pi => pi.ParameterType == typeof(ILogger));
                    if (usesLogger) {
                        // Attach updater
                        registration.PipelineBuilding += (sender, pipeline) => {
                            pipeline.Use(new LoggerInjector(registration.Activator.LimitType));
                        };
                    }
                }
                catch (NoConstructorsFoundException) {
                    return;
                }
            }
        }

        private class LoggerInjector : IResolveMiddleware {

            /// <inheritdoc/>
            public PipelinePhase Phase => PipelinePhase.ParameterSelection;

            public LoggerInjector(Type type) {
                _type = type;
            }

            /// <inheritdoc/>
            public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next) {
                var type = typeof(ILogger<>).MakeGenericType(_type);
                var logger = (ILogger)context.Resolve(type);
                context.ChangeParameters(new[] { TypedParameter.From(logger) }
                    .Concat(context.Parameters));
                // Continue the resolve.
                next(context);
            }

            private readonly Type _type;
        }
    }
}
