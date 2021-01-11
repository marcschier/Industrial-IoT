﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Serializers {
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Autofac;

    /// <summary>
    /// All pluggable serializers
    /// </summary>
    public class NewtonSoftJsonModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<NewtonSoftJsonSerializer>()
                .AsImplementedInterfaces();
            builder.RegisterType<NewtonSoftJsonConverters>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}