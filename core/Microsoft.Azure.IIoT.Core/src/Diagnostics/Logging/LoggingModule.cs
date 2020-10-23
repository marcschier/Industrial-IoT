// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Extensions.Logging;
    using Autofac;
    using Autofac.Core;
    using Autofac.Core.Activators.Reflection;
    using Autofac.Core.Registration;
    using Autofac.Core.Resolving.Pipeline;
    using Module = Autofac.Module;
    using System;
    using System.Linq;

    /// <summary>
    /// Logger provider module
    /// </summary>
    public class LoggingModule : Module {

        /// <inheritdoc/>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder registry,
            IComponentRegistration registration) {
            // Ignore components that provide loggers (and thus avoid a circular dependency below)
            if (registration.Services
                .OfType<TypedService>()
                .Any(ts => typeof(ILogger).IsAssignableFrom(ts.ServiceType) ||
                           ts.ServiceType == typeof(ILoggerFactory))) {
                return;
            }
            if (registration.Activator is ReflectionActivator ra) {
                try {
                    var ctors = ra.ConstructorFinder.FindConstructors(ra.LimitType);
                    var usesLogger = ctors
                        .SelectMany(ctor => ctor.GetParameters())
                        .Any(pi => pi.ParameterType == typeof(ILogger)); // non-generic only
                    // Ignore components known to be without logger dependencies
                    if (!usesLogger) {
                        return;
                    }
                }
                catch (NoConstructorsFoundException) {
                    return; // No need
                }
            }
            registration.PipelineBuilding += (sender, pipeline) => {
                pipeline.Use(new LoggerUpdater(registration.Activator.LimitType));
            };
        }

        private class LoggerUpdater : IResolveMiddleware {

            /// <inheritdoc/>
            public PipelinePhase Phase => PipelinePhase.ParameterSelection;

            public LoggerUpdater(Type limitType) {
                _limitType = limitType;
            }

            /// <inheritdoc/>
            public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next) {
                // Add our parameters.
                var type = typeof(ILogger<>).MakeGenericType(_limitType);
                var loggerToInject = (ILogger)context.Resolve(type);
                context.ChangeParameters(new[] { TypedParameter.From(loggerToInject) }
                    .Concat(context.Parameters));
                // Continue the resolve.
                next(context);
            }

            private readonly Type _limitType;
        }
    }
}
