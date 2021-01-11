// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {
    using Autofac;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Create lifetime scope
    /// </summary>
    public static class LifetimeScopeEx {

        /// <summary>
        /// Create identity lifetime scope
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="deviceId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ILifetimeScope> CreateDeviceScopeAsync(
            this ILifetimeScope parent, string deviceId, CancellationToken ct = default) {
            return CreateDeviceScopeAsync(parent, deviceId, _ => { }, _ => { },  ct);
        }

        /// <summary>
        /// Create identity lifetime scope
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="deviceId"></param>
        /// <param name="configureContainer"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ILifetimeScope> CreateDeviceScopeAsync(
            this ILifetimeScope parent, string deviceId, Action<ContainerBuilder> configureContainer,
            CancellationToken ct = default) {
            return CreateDeviceScopeAsync(parent, deviceId, configureContainer, _ => { }, ct);
        }

        /// <summary>
        /// Create identity lifetime scope
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="deviceId"></param>
        /// <param name="configureOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ILifetimeScope> CreateDeviceScopeAsync(
            this ILifetimeScope parent, string deviceId, Action<IoTEdgeClientOptions> configureOptions,
            CancellationToken ct = default) {
            return CreateDeviceScopeAsync(parent, deviceId, _ => { }, configureOptions, ct);
        }

        /// <summary>
        /// Create identity lifetime scope
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="deviceId"></param>
        /// <param name="configureContainer"></param>
        /// <param name="configureOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<ILifetimeScope> CreateDeviceScopeAsync(
            this ILifetimeScope parent, string deviceId, Action<ContainerBuilder> configureContainer,
            Action<IoTEdgeClientOptions> configureOptions, CancellationToken ct = default) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }
            if (configureContainer == null) {
                throw new ArgumentNullException(nameof(configureContainer));
            }
            if (configureOptions == null) {
                throw new ArgumentNullException(nameof(configureOptions));
            }
            var identityProvider = parent.Resolve<IIoTEdgeIdentityProvider>();
            var cs = await identityProvider.GetConnectionStringAsync(deviceId, ct);
            return parent.BeginLifetimeScope(builder => {
                configureContainer(builder);
                builder.Configure<IoTEdgeClientOptions>(options => {
                    configureOptions(options);
                    options.EdgeHubConnectionString = cs;
                });
            });
        }
    }
}
