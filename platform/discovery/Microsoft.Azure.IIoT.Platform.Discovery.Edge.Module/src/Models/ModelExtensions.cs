﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Edge.Module.Models {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;

    /// <summary>
    /// Discovery model extensionis
    /// </summary>
    public static class ModelExtensions {

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static DiscoveryCancelInternalApiModel ToApiModel(
            this DiscoveryCancelModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryCancelInternalApiModel {
                Id = model.Id,
                Context = model.Context.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryCancelModel ToServiceModel(
            this DiscoveryCancelInternalApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryCancelModel {
                Id = model.Id,
                Context = model.Context.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static DiscoveryConfigApiModel ToApiModel(
            this DiscoveryConfigModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryConfigApiModel {
                AddressRangesToScan = model.AddressRangesToScan,
                NetworkProbeTimeout = model.NetworkProbeTimeout,
                MaxNetworkProbes = model.MaxNetworkProbes,
                PortRangesToScan = model.PortRangesToScan,
                PortProbeTimeout = model.PortProbeTimeout,
                MaxPortProbes = model.MaxPortProbes,
                MinPortProbesPercent = model.MinPortProbesPercent,
                IdleTimeBetweenScans = model.IdleTimeBetweenScans,
                Locales = model.Locales,
                DiscoveryUrls = model.DiscoveryUrls,
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryConfigModel ToServiceModel(
            this DiscoveryConfigApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryConfigModel {
                AddressRangesToScan= model.AddressRangesToScan,
                NetworkProbeTimeout= model.NetworkProbeTimeout,
                MaxNetworkProbes= model.MaxNetworkProbes,
                PortRangesToScan= model.PortRangesToScan,
                PortProbeTimeout= model.PortProbeTimeout,
                MaxPortProbes= model.MaxPortProbes,
                MinPortProbesPercent= model.MinPortProbesPercent,
                IdleTimeBetweenScans= model.IdleTimeBetweenScans,
                Locales= model.Locales,
                DiscoveryUrls= model.DiscoveryUrls,
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static DiscoveryRequestInternalApiModel ToApiModel(
            this DiscoveryRequestModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryRequestInternalApiModel {
                Id = model.Id,
                Configuration = model.Configuration.ToApiModel(),
                Discovery = (IIoT.Platform.Registry.Api.Models.DiscoveryMode?)model.Discovery,
                Context = model.Context.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryRequestModel ToServiceModel(
            this DiscoveryRequestInternalApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryRequestModel {
                Id = model.Id,
                Configuration = model.Configuration.ToServiceModel(),
                Discovery = (IIoT.Platform.Registry.Models.DiscoveryMode?)model.Discovery,
                Context = model.Context.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static RegistryOperationContextApiModel ToApiModel(
            this RegistryOperationContextModel model) {
            if (model == null) {
                return null;
            }
            return new RegistryOperationContextApiModel {
                AuthorityId = model.AuthorityId,
                Time = model.Time
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static RegistryOperationContextModel ToServiceModel(
            this RegistryOperationContextApiModel model) {
            if (model == null) {
                return null;
            }
            return new RegistryOperationContextModel {
                AuthorityId= model.AuthorityId,
                Time= model.Time
            };
        }
    }
}

