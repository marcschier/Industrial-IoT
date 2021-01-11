// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.DependencyInjection {
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.IIoT.Azure.SignalR;
    using Microsoft.Azure.SignalR;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// SignalR setup extensions
    /// </summary>
    public static class SignalRBuilderEx {

        /// <summary>
        /// Add azure signalr if possible
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddAzureSignalRService(this ISignalRServerBuilder builder,
            SignalRServiceOptions options = null) {
            if (options == null) {
                options = builder.Services.BuildServiceProvider().GetService<IOptions<SignalRServiceOptions>>()?.Value;
            }
            if (string.IsNullOrEmpty(options?.ConnectionString) || options.IsServerLess) {
                // not using signalr service because of legacy configuration.
                return builder;
            }
            builder.AddAzureSignalR().Services.Configure((Action<ServiceOptions>)(serviceOptions => {
                serviceOptions.ConnectionString = options.ConnectionString;
                serviceOptions.ClaimsProvider = context => context.User.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier);
            }));
            return builder;
        }
    }
}
