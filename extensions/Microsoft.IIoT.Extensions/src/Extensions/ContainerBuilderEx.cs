﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.Extensions.DependencyInjection;
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
    }
}
