// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans {
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Autofac.Extensions.DependencyInjection;
    using Autofac;
    using System.Reflection;
    using global::Orleans;
    using global::Orleans.Hosting;
    using global::Orleans.ApplicationParts;

    /// <summary>
    /// Startup configuration
    /// </summary>
    public abstract class OrleansStartup {

        /// <summary>
        /// Configure silo
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal virtual void ConfigureSilo(ISiloBuilder builder) {
            builder
                .ConfigureServices(services => Configure(services))
                .ConfigureLogging(logging => Configure(logging))
                .ConfigureApplicationParts(parts => Configure(parts))
                ;
            Configure(builder);
        }

        /// <summary>
        /// Configure host
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal virtual void ConfigureHost(IHostBuilder builder) {
            builder
                .UseServiceProviderFactory(
                    new AutofacServiceProviderFactory(builder => Configure(builder)))
                ;
            Configure(builder);
        }

        /// <summary>
        /// Configure client builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal virtual void ConfigureClient(IClientBuilder builder) {
            builder
                .ConfigureServices(services => Configure(services))
                .ConfigureLogging(logging => Configure(logging))
                .ConfigureApplicationParts(parts => Configure(parts))
                .UseServiceProviderFactory(
                    new AutofacServiceProviderFactory(builder => Configure(builder)))
                ;
            Configure(builder);
        }

        /// <summary>
        /// Configure host
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(IHostBuilder builder) {
        }

        /// <summary>
        /// Configure client
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(IClientBuilder builder) {
        }

        /// <summary>
        /// Configure silo
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(ISiloBuilder builder) {
        }

        /// <summary>
        /// Configure logging
        /// </summary>
        /// <param name="logging"></param>
        protected virtual void Configure(ILoggingBuilder logging) {
            logging
                .AddConsole()
                .AddDebug();
        }

        /// <summary>
        /// Configure container builder
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(ContainerBuilder builder) {
            GetType().GetMethod("ConfigureContainer")?
                .Invoke(this, new object[] { builder });
        }

        /// <summary>
        /// Configure application parts
        /// </summary>
        /// <param name="parts"></param>
        protected virtual void Configure(IApplicationPartManager parts) {
            parts
                .AddApplicationPart(Assembly.GetEntryAssembly())
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddApplicationPart(GetType().Assembly)
                .WithReferences()
                ;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        protected virtual void Configure(IServiceCollection services) {
            GetType().GetMethod("ConfigureServices")?
                .Invoke(this, new object[] { services });
        }
    }
}