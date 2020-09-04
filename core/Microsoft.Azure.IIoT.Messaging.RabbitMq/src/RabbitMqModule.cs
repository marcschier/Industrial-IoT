// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq {
    using Microsoft.Azure.IIoT.Messaging.MassTransit;
    using global::MassTransit;
    using global::MassTransit.AutofacIntegration;

    /// <summary>
    /// Injected RabbitMq bus
    /// </summary>
    public class RabbitMqModule : MassTransitModule {

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilderBusConfigurator configure) {
            // add the bus to the container
            configure.UsingRabbitMq((ctx, option) => {
                var configuration = ctx.GetRequiredService<IRabbitMqConfig>();

                option.Host(configuration.HostName, x => {
                    x.Password(configuration.Key);
                    x.Username(configuration.UserName);
                });

                // TODO Add more

                option.ConfigureEndpoints(ctx);
            });
        }
    }
}
