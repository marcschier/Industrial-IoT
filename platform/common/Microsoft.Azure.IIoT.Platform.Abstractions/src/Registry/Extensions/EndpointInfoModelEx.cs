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
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoModel SetEndpointId(this EndpointInfoModel model) {
            if (model == null) {
                return null;
            }
            if (string.IsNullOrEmpty(model.ApplicationId)) {
                throw new ArgumentException("Missing application id", nameof(model));
            }
            if (string.IsNullOrEmpty(model.Endpoint?.Url)) {
                throw new ArgumentException("Missing Endpoint Url", nameof(model));
            }
            var url = model.Endpoint.Url.ToUpperInvariant();
            var mode = model.Endpoint.SecurityMode;
            if (!mode.HasValue || mode.Value == SecurityMode.None) {
                mode = SecurityMode.Best;
            }
            var securityPolicy = model.Endpoint.SecurityPolicy?.ToUpperInvariant() ?? "";
            var id = $"{url}-{model.ApplicationId}-{mode}-{securityPolicy}";
            model.Id = kEndpointPrefix + id.ToSha256Hash();
            return model;
        }

        /// <summary>
        /// Checks whether the identifier is an endpoint id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsEndpointId(string id) {
            var endpointId = Try.Op(() => HubResource.Parse(id, out _, out _));
            if (endpointId == null || !endpointId.StartsWith(kEndpointPrefix)) {
                return false;
            }
            return endpointId[3..].IsBase16();
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
            return model.SetEqualsSafe(that, (x, y) => x.IsSameAs(y));
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
            return
                model.Endpoint.HasSameSecurityProperties(that.Endpoint) &&
                model.Endpoint?.Url == that.Endpoint?.Url &&
                model.AuthenticationMethods.IsSameAs(that.AuthenticationMethods) &&
                model.DiscovererId == that.DiscovererId &&
                model.Visibility == that.Visibility &&
                model.SecurityLevel == that.SecurityLevel;
        }

        /// <summary>
        /// Is disabled
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsNotSeen(this EndpointInfoModel model) {
            if (model == null) {
                return true;
            }
            return model.Visibility != EntityVisibility.Found;
        }

        /// <summary>
        /// Disable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoModel SetNotSeen(this EndpointInfoModel model) {
            if (model != null) {
                model.Visibility = EntityVisibility.NotSeen;
                model.NotSeenSince = DateTime.UtcNow;
            }
            return model;
        }

        /// <summary>
        /// Enable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoModel SetAsFound(this EndpointInfoModel model) {
            if (model != null) {
                model.Visibility = EntityVisibility.Found;
                model.NotSeenSince = null;
            }
            return model;
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
                Visibility = model.Visibility,
                Endpoint = model.Endpoint.Clone(),
                Id = model.Id,
                AuthenticationMethods = model.AuthenticationMethods?
                    .Select(c => c.Clone())
                    .ToList(),
                SecurityLevel = model.SecurityLevel,
                Created = model.Created.Clone(),
                Updated = model.Updated.Clone(),
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
            if (endpoint == null) {
                return model;
            }
            if (model == null) {
                return endpoint;
            }
            endpoint.ApplicationId = model.ApplicationId;
            endpoint.NotSeenSince = model.NotSeenSince;
            endpoint.Visibility = model.Visibility;
            endpoint.Endpoint = model.Endpoint.Clone();
            endpoint.Id = model.Id;
            endpoint.AuthenticationMethods = model.AuthenticationMethods?
                .Select(c => c.Clone())
                .ToList();
            endpoint.SecurityLevel = model.SecurityLevel;
            endpoint.DiscovererId = model.DiscovererId;
            endpoint.Updated = model.Updated;
            return endpoint;
        }

        /// <summary>
        /// Compares for logical equality
        /// </summary>
        private class LogicalEquality : IEqualityComparer<EndpointInfoModel> {

            /// <inheritdoc />
            public bool Equals(EndpointInfoModel x, EndpointInfoModel y) {
                if (x == y) {
                    return true;
                }
                if (x == null || y == null) {
                    return false;
                }
                if (!(x.Endpoint?.Url).EqualsIgnoreCase(y.Endpoint?.Url)) {
                    return false;
                }
                if (x.ApplicationId != y.ApplicationId) {
                    return false;
                }
                if (x.Endpoint?.SecurityPolicy !=
                    y.Endpoint?.SecurityPolicy) {
                    return false;
                }
                if (x.Endpoint?.SecurityMode !=
                    y.Endpoint?.SecurityMode) {
                    return false;
                }
                return true;
            }

            /// <inheritdoc />
            public int GetHashCode(EndpointInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj?.ApplicationId);
                hash.Add(obj?.Endpoint?.Url?.ToUpperInvariant());
                hash.Add(obj?.Endpoint?.SecurityMode);
                hash.Add(obj?.Endpoint?.SecurityPolicy);
                return hash.ToHashCode();
            }
        }

        private const string kEndpointPrefix = "uat";
    }
}
