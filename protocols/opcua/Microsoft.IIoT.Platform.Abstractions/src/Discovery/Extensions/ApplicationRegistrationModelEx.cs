// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Service model extensions for discovery service
    /// </summary>
    public static class ApplicationRegistrationModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<ApplicationRegistrationModel> model,
            IEnumerable<ApplicationRegistrationModel> that) {
            if (model == that) {
                return true;
            }
            return model.SetEqualsSafe(that, (x, y) => x.IsSameAs(y));
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this ApplicationRegistrationModel model,
            ApplicationRegistrationModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (!that.Endpoints.IsSameAs(model.Endpoints)) {
                return false;
            }
            if (!that.Application.IsSameAs(model.Application)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationModel Clone(this ApplicationRegistrationModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationModel {
                Application = model.Application.Clone(),
                Endpoints = model.Endpoints?.Select(e => e.Clone()).ToList()
            };
        }

        /// <summary>
        /// Add or update a server list
        /// </summary>
        /// <param name="discovered"></param>
        /// <param name="server"></param>
        public static void AddOrUpdate(this List<ApplicationRegistrationModel> discovered,
            ApplicationRegistrationModel server) {
            if (discovered is null) {
                throw new System.ArgumentNullException(nameof(discovered));
            }

            var actual = discovered
                .FirstOrDefault(s => s.Application.IsSameAs(server.Application));
            if (actual != null) {
                // Merge server info
                actual.UnionWith(server);
            }
            else {
                discovered.Add(server);
            }
        }

        /// <summary>
        /// Create Union with server
        /// </summary>
        /// <param name="model"></param>
        /// <param name="server"></param>
        public static void UnionWith(this ApplicationRegistrationModel model,
            ApplicationRegistrationModel server) {
            if (server is null) {
                throw new System.ArgumentNullException(nameof(server));
            }
            if (model is null) {
                throw new System.ArgumentNullException(nameof(model));
            }

            if (model.Application == null) {
                model.Application = server.Application;
            }
            else {
                model.Application.Capabilities = model.Application.Capabilities.MergeWith(
                    server?.Application?.Capabilities);
                model.Application.DiscoveryUrls = model.Application.DiscoveryUrls.MergeWith(
                    server?.Application?.DiscoveryUrls);
                model.Application.HostAddresses = model.Application.HostAddresses.MergeWith(
                    server?.Application?.HostAddresses);
            }

            if (server?.Endpoints?.Any() ?? false) {
                if (model.Endpoints == null) {
                    model.Endpoints = server.Endpoints;
                }
                else {
                    var endpoints = new List<EndpointInfoModel>(model.Endpoints);
                    foreach (var ep in server.Endpoints) {
                        var found = model.Endpoints.Where(ep.IsSameAs);
                        if (!found.Any()) {
                            endpoints.Add(ep);
                        }
                        foreach (var existing in found) {
                            if (existing.Endpoint == null) {
                                existing.Endpoint = ep.Endpoint;
                                continue;
                            }
                            existing.Endpoint?.UnionWith(ep.Endpoint);
                        }
                    }
                    model.Endpoints = endpoints;
                }
            }
        }
    }
}