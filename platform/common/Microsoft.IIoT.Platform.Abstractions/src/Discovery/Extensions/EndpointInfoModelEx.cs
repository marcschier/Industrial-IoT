// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Utils;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Service model extensions for discovery service
    /// </summary>
    public static class EndpointInfoModelEx {

        /// <summary>
        /// Logical comparison of endpoint registrations
        /// </summary>
        public static IEqualityComparer<EndpointInfoModel> Logical =>
            new LogicalComparer();

        /// <summary>
        /// Logical comparison of endpoint registrations
        /// </summary>
        public static IEqualityComparer<EndpointInfoModel> Structural =>
            new StructuralComparer();

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
        /// Patch endpoint
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="patch"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Patch(this EndpointInfoModel existing,
            EndpointInfoModel patch, out EndpointInfoModel result) {
            if (existing == null) {
                result = patch;
                return true;
            }
            if (patch == null) {
                result = existing;
                return false; // no changes
            }
            if (!Structural.Equals(existing, patch)) {

                result = existing;
                result.ApplicationId = patch.ApplicationId;
                result.NotSeenSince = patch.NotSeenSince;
                result.Visibility = patch.Visibility;
                result.Endpoint = patch.Endpoint.Clone();
                result.Id = patch.Id;
                result.AuthenticationMethods = patch.AuthenticationMethods?
                    .Select(c => c.Clone())
                    .ToList();
                result.SecurityLevel = patch.SecurityLevel;
                result.DiscovererId = patch.DiscovererId;

                result.Updated = patch.Updated;
                return true;
            }
            result = existing;
            return false;
        }

        /// <summary>
        /// Is disabled
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsLost(this EndpointInfoModel model) {
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
        public static EndpointInfoModel SetAsLost(this EndpointInfoModel model) {
            if (model != null) {
                model.Visibility = EntityVisibility.Lost;
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
        /// Compares for logical equality
        /// </summary>
        private class LogicalComparer : IEqualityComparer<EndpointInfoModel> {

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

        /// <summary>
        /// Compares for logical equality - applications are logically equivalent if they
        /// have the same uri, type, and site location or supervisor that registered.
        /// </summary>
        private class StructuralComparer : IEqualityComparer<EndpointInfoModel> {

            /// <inheritdoc />
            public bool Equals(EndpointInfoModel x, EndpointInfoModel y) {
                if (x == y) {
                    return true;
                }
                if (x == null || y == null) {
                    return false;
                }
                return
                    x.ApplicationId == y.ApplicationId &&
                    x.DiscovererId == y.DiscovererId &&
                    x.Id == y.Id &&
                    x.SecurityLevel == y.SecurityLevel &&
                    x.Visibility == y.Visibility &&
                    x.NotSeenSince == y.NotSeenSince &&
                    x.Endpoint.IsSameAs(y.Endpoint);
            }

            /// <inheritdoc />
            public int GetHashCode(EndpointInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj.ApplicationId);
                hash.Add(obj.DiscovererId);
                hash.Add(obj.Id);
                hash.Add(obj.SecurityLevel);
                hash.Add(obj.Visibility);
                hash.Add(obj.NotSeenSince);
                hash.Add(obj.Endpoint.CreateConsistentHash());
                return hash.ToHashCode();
            }
        }

        private const string kEndpointPrefix = "uat";
    }
}
