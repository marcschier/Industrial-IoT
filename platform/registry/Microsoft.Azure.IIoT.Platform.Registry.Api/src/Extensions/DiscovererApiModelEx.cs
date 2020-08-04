// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {

    /// <summary>
    /// Handle event
    /// </summary>
    public static class DiscovererApiModelEx {

        /// <summary>
        /// Update a discover
        /// </summary>
        /// <param name="discoverer"></param>
        /// <param name="update"></param>
        public static DiscovererApiModel Patch(this DiscovererApiModel update,
            DiscovererApiModel discoverer) {
            if (update == null) {
                return discoverer;
            }
            if (discoverer == null) {
                discoverer = new DiscovererApiModel();
            }
            discoverer.Connected = update.Connected;
            discoverer.Discovery = update.Discovery;
            discoverer.Id = update.Id;
            discoverer.LogLevel = update.LogLevel;
            discoverer.OutOfSync = update.OutOfSync;
            discoverer.RequestedMode = update.RequestedMode;
            discoverer.RequestedConfig = (update.RequestedConfig ?? new DiscoveryConfigApiModel())
                .Patch(discoverer.RequestedConfig);
            discoverer.DiscoveryConfig = (update.DiscoveryConfig ?? new DiscoveryConfigApiModel())
                .Patch(discoverer.DiscoveryConfig);
            discoverer.Version = update.Version;
            return discoverer;
        }
    }
}
