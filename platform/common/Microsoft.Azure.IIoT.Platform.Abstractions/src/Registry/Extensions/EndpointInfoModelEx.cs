// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Exceptions;

    /// <summary>
    /// Service model extensions for discovery service
    /// </summary>
    public static class EndpointInfoModelEx {

        /// <summary>
        /// Logical comparison of endpoint registrations
        /// </summary>
        public static IEqualityComparer<EndpointInfoModel> Logical =>
            new LogicalEquality();

        /// <summary>
        /// Create unique endpoint
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="url"></param>
        /// <param name="mode"></param>
        /// <param name="securityPolicy"></param>
        /// <returns></returns>
        public static string CreateEndpointId(string applicationId, string url,
            SecurityMode? mode, string securityPolicy) {
            if (applicationId == null || url == null) {
                return null;
            }

            url = url.ToLowerInvariant();

            if (!mode.HasValue || mode.Value == SecurityMode.None) {
                mode = SecurityMode.Best;
            }
            securityPolicy = securityPolicy?.ToLowerInvariant() ?? "";

            var id = $"{url}-{applicationId}-{mode}-{securityPolicy}";
            return "uat" + id.ToSha1Hash();
        }

        /// <summary>
        /// Checks whether the identifier is an endpoint id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsEndpointId(string id) {
            var endpointId = Try.Op(() => HubResource.Parse(id, out _, out _));
            if (endpointId == null || !endpointId.StartsWith("uat")) {
                return false;
            }
            return endpointId.Substring(3).IsBase16();
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<EndpointInfoModel> model,
            IEnumerable<EndpointInfoModel> that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (model.Count() != that.Count()) {
                return false;
            }
            return model.All(a => that.Any(b => b.IsSameAs(a)));
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this EndpointInfoModel model,
            EndpointInfoModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            return model.Endpoint.HasSameSecurityProperties(that.Endpoint) &&
                model.EndpointUrl == that.EndpointUrl &&
                model.AuthenticationMethods.IsSameAs(that.AuthenticationMethods) &&
                model.SiteId == that.SiteId &&
                model.DiscovererId == that.DiscovererId &&
                model.SupervisorId == that.SupervisorId &&
                model.SecurityLevel == that.SecurityLevel;
        }

        /// <summary>
        /// Is activated
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsActivated(this EndpointInfoModel model) {
            if (model == null) {
                return false;
            }
            return
                model.ActivationState == EntityActivationState.Activated ||
                model.ActivationState == EntityActivationState.ActivatedAndConnected;
        }

        /// <summary>
        /// Is connected
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsConnected(this EndpointInfoModel model) {
            if (model == null) {
                return false;
            }
            return
                model.ActivationState == EntityActivationState.ActivatedAndConnected;
        }

        /// <summary>
        /// Is disabled
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsDisabled(this EndpointInfoModel model) {
            if (model == null) {
                return true;
            }
            return
                model.NotSeenSince != null;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoModel Clone(this EndpointInfoModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoModel {
                ApplicationId = model.ApplicationId,
                GenerationId = model.GenerationId,
                NotSeenSince = model.NotSeenSince,
                ActivationState = model.ActivationState,
                EndpointState = model.EndpointState,
                Endpoint = model.Endpoint.Clone(),
                EndpointUrl = model.EndpointUrl,
                Id = model.Id,
                AuthenticationMethods = model.AuthenticationMethods?
                    .Select(c => c.Clone()).ToList(),
                SecurityLevel = model.SecurityLevel,
                SiteId = model.SiteId,
                SupervisorId = model.SupervisorId,
                DiscovererId = model.DiscovererId
            };
        }

        /// <summary>
        /// Patch endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="model"></param>
        public static EndpointInfoModel Patch(this EndpointInfoModel endpoint,
            EndpointInfoModel model) {
            endpoint.ApplicationId = model.ApplicationId;
            endpoint.NotSeenSince = model.NotSeenSince;
            endpoint.ActivationState = model.ActivationState;
            endpoint.EndpointState = model.EndpointState;
            endpoint.Endpoint = model.Endpoint.Clone();
            endpoint.EndpointUrl = model.EndpointUrl;
            endpoint.Id = model.Id;
            endpoint.AuthenticationMethods = model.AuthenticationMethods?
                .Select(c => c.Clone()).ToList();
            endpoint.SecurityLevel = model.SecurityLevel;
            endpoint.SiteId = model.SiteId;
            endpoint.SupervisorId = model.SupervisorId;
            endpoint.DiscovererId = model.DiscovererId;
            return endpoint;
        }

        /// <summary>
        /// Get site or gateway id from endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static string GetSiteOrGatewayId(this EndpointInfoModel endpoint) {
            if (endpoint == null) {
                return null;
            }
            var siteOrGatewayId = endpoint?.SiteId;
            if (siteOrGatewayId == null) {
                var id = endpoint?.DiscovererId ?? endpoint?.SupervisorId;
                if (id != null) {
                    siteOrGatewayId = HubResource.Parse(id, out _, out _);
                }
            }
            return siteOrGatewayId;
        }

        /// <summary>
        /// Compares for logical equality
        /// </summary>
        private class LogicalEquality : IEqualityComparer<EndpointInfoModel> {

            /// <inheritdoc />
            public bool Equals(EndpointInfoModel x, EndpointInfoModel y) {
                if (!x.EndpointUrl.EqualsIgnoreCase(y.EndpointUrl)) {
                    return false;
                }
                if (x.ApplicationId != y.ApplicationId) {
                    return false;
                }
                if (x?.Endpoint.SecurityPolicy !=
                    y?.Endpoint.SecurityPolicy) {
                    return false;
                }
                if (x?.Endpoint.SecurityMode !=
                    y?.Endpoint.SecurityMode) {
                    return false;
                }
                return true;
            }

            /// <inheritdoc />
            public int GetHashCode(EndpointInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj.ApplicationId);
                hash.Add(obj?.EndpointUrl?.ToLowerInvariant());
                hash.Add(obj?.Endpoint.SecurityMode);
                hash.Add(obj?.Endpoint.SecurityPolicy);
                return hash.ToHashCode();
            }
        }
    }
}
