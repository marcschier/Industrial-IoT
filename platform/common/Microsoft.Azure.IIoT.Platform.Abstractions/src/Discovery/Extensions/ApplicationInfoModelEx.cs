// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Service model extensions for discovery service
    /// </summary>
    public static class ApplicationInfoModelEx {

        /// <summary>
        /// Get logical equality comparer
        /// </summary>
        public static IEqualityComparer<ApplicationInfoModel> Logical { get; } =
            new LogicalComparer();

        /// <summary>
        /// Get structural equality comparer
        /// </summary>
        public static IEqualityComparer<ApplicationInfoModel> Structural { get; } =
            new StructuralComparer();

        /// <summary>
        /// Set unique application id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoModel SetApplicationId(this ApplicationInfoModel model) {
            if (model == null) {
                return null;
            }
            var applicationUri = model.ApplicationUri;
            if (string.IsNullOrEmpty(applicationUri)) {
                throw new ArgumentException("Missing application uri", nameof(model));
            }
            var discovererId = model.DiscovererId;
            var applicationType = model.ApplicationType;
            applicationUri = applicationUri.ToUpperInvariant();
            var id = $"{discovererId ?? ""}-{applicationType}-{applicationUri}";
            var prefix = applicationType == ApplicationType.Client ? "uac" : "uas";
            model.ApplicationId = prefix + id.ToSha256Hash();
            return model;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<ApplicationInfoModel> model,
            IEnumerable<ApplicationInfoModel> that) {
            if (model == that) {
                return true;
            }
            return model.SetEqualsSafe(that, (x, y) => x.IsSameAs(y));
        }

        /// <summary>
        /// Is not seen
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsLost(this ApplicationInfoModel model) {
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
        public static ApplicationInfoModel SetAsLost(this ApplicationInfoModel model) {
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
        public static ApplicationInfoModel SetAsFound(this ApplicationInfoModel model) {
            if (model != null) {
                model.Visibility = EntityVisibility.Found;
                model.NotSeenSince = null;
            }
            return model;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this ApplicationInfoModel model,
            ApplicationInfoModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            return
                that.Visibility == model.Visibility &&
                that.ApplicationUri.EqualsIgnoreCase(model.ApplicationUri) &&
                that.ApplicationType == model.ApplicationType;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoModel Clone(this ApplicationInfoModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoModel {
                GenerationId = model.GenerationId,
                ApplicationId = model.ApplicationId,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ApplicationType = model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                Capabilities = model.Capabilities
                    .ToHashSetSafe(),
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                HostAddresses = model.HostAddresses
                    .ToHashSetSafe(),
                DiscoveryUrls = model.DiscoveryUrls
                    .ToHashSetSafe(),
                NotSeenSince = model.NotSeenSince,
                Visibility = model.Visibility,
                ProductUri = model.ProductUri,
                GatewayServerUri = model.GatewayServerUri,
                Created = model.Created.Clone(),
                Updated = model.Updated.Clone(),
                DiscovererId = model.DiscovererId
            };
        }

        /// <summary>
        /// Convert to registration request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationRequestModel ToRegistrationRequest(
            this ApplicationInfoModel model) {
            if (model is null) {
                return null;
            }
            return new ApplicationRegistrationRequestModel {
                ApplicationName = model.ApplicationName,
                ApplicationType = model.ApplicationType,
                Capabilities = model.Capabilities,
                ApplicationUri = model.ApplicationUri,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                LocalizedNames = model.LocalizedNames,
                Locale = model.Locale,
                ProductUri = model.ProductUri,
            };
        }

        /// <summary>
        /// Convert registration request to application info model
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationInfoModel ToApplicationInfo(
            this ApplicationRegistrationRequestModel request,
            OperationContextModel context) {
            if (request is null) {
                return null;
            }
            return new ApplicationInfoModel {
                ApplicationName = request.ApplicationName,
                LocalizedNames = request.LocalizedNames,
                ProductUri = request.ProductUri,
                DiscoveryUrls = request.DiscoveryUrls,
                DiscoveryProfileUri = request.DiscoveryProfileUri,
                ApplicationType = request.ApplicationType ?? ApplicationType.Server,
                ApplicationUri = request.ApplicationUri,
                Locale = request.Locale,
                Capabilities = request.Capabilities,
                GatewayServerUri = request.GatewayServerUri,
                Created = context,
                Visibility = EntityVisibility.Unknown,
                NotSeenSince = null,
                GenerationId = null,
                Updated = null,
                ApplicationId = null,
                DiscovererId = null,
                HostAddresses = null,
            };
        }

        /// <summary>
        /// Patch application
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="patch"></param>
        /// <param name="result"></param>
        public static bool Patch(this ApplicationInfoModel existing,
            ApplicationInfoModel patch, out ApplicationInfoModel result) {
            if (patch == null) {
                result = existing;
                return true;
            }
            if (existing == null) {
                result = patch;
                return false;
            }
            if (!Structural.Equals(existing, patch)) {
                result = existing;
                result.ApplicationId = patch.ApplicationId;
                result.ApplicationName = patch.ApplicationName;
                result.LocalizedNames = patch.LocalizedNames;
                result.ApplicationType = patch.ApplicationType;
                result.ApplicationUri = patch.ApplicationUri;
                result.Capabilities = patch.Capabilities;
                result.DiscoveryProfileUri = patch.DiscoveryProfileUri;
                result.HostAddresses = patch.HostAddresses;
                result.DiscoveryUrls = patch.DiscoveryUrls;
                result.Visibility = patch.Visibility;
                result.NotSeenSince = patch.NotSeenSince;
                result.ProductUri = patch.ProductUri;
                result.DiscovererId = patch.DiscovererId;
                result.GatewayServerUri = patch.GatewayServerUri;
                result.Locale = patch.Locale;

                result.Updated = patch.Updated;
                return true;
            }
            result = existing;
            return false;
        }

        /// <summary>
        /// Returns an application name from either application name field or
        /// localized text dictionary
        /// </summary>
        /// <param name="model">The application model.</param>
        public static string GetApplicationName(this ApplicationInfoModel model) {
            if (model is null) {
                return null;
            }
            if (!string.IsNullOrWhiteSpace(model.ApplicationName)) {
                return model.ApplicationName;
            }
            return model.LocalizedNames?
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n.Value)).Value;
        }

        /// <summary>
        /// Compares for logical equality - applications are logically equivalent if they
        /// have the same uri, type, and site location or supervisor that registered.
        /// </summary>
        private class LogicalComparer : IEqualityComparer<ApplicationInfoModel> {

            /// <inheritdoc />
            public bool Equals(ApplicationInfoModel x, ApplicationInfoModel y) {
                if (x.DiscovererId != y.DiscovererId) {
                    return false;
                }
                if (x.ApplicationType != y.ApplicationType) {
                    return false;
                }
                if (!x.ApplicationUri.EqualsIgnoreCase(y.ApplicationUri)) {
                    return false;
                }
                return true;
            }

            /// <inheritdoc />
            public int GetHashCode(ApplicationInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj.ApplicationType);
                hash.Add(obj.ApplicationUri?.ToUpperInvariant());
                hash.Add(obj.DiscovererId);
                return hash.ToHashCode();
            }
        }

        /// <summary>
        /// Compares for logical equality - applications are logically equivalent if they
        /// have the same uri, type, and site location or supervisor that registered.
        /// </summary>
        private class StructuralComparer : IEqualityComparer<ApplicationInfoModel> {

            /// <inheritdoc />
            public bool Equals(ApplicationInfoModel x, ApplicationInfoModel y) {
                if (x == y) {
                    return true;
                }
                if (x == null || y == null) {
                    return false;
                }
                return
                    x.ApplicationId == y.ApplicationId &&
                    x.DiscovererId == y.DiscovererId &&
                    x.ApplicationType == y.ApplicationType &&
                    x.ApplicationUri.EqualsIgnoreCase(y.ApplicationUri) &&
                    x.DiscoveryProfileUri == y.DiscoveryProfileUri &&
                    x.GatewayServerUri == y.GatewayServerUri &&
                    x.ProductUri == y.ProductUri &&
                    x.Locale == y.Locale &&
                    x.HostAddresses.SetEqualsSafe(y.HostAddresses) &&
                    x.Visibility == y.Visibility &&
                    x.NotSeenSince == y.NotSeenSince &&
                    x.ApplicationName == y.ApplicationName &&
                    x.LocalizedNames.DictionaryEqualsSafe(y.LocalizedNames) &&
                    x.Capabilities.SetEqualsSafe(y.Capabilities) &&
                    x.DiscoveryUrls.SetEqualsSafe(y.DiscoveryUrls);
            }

            /// <inheritdoc />
            public int GetHashCode(ApplicationInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj.ApplicationId);
                hash.Add(obj.DiscovererId);
                hash.Add(obj.ApplicationType);
                hash.Add(obj.ApplicationUri?.ToUpperInvariant());
                hash.Add(obj.DiscoveryProfileUri);
                hash.Add(obj.GatewayServerUri);
                hash.Add(obj.ProductUri);
                hash.Add(obj.Locale);
                hash.Add(obj.Visibility);
                hash.Add(obj.NotSeenSince);
                hash.Add(obj.ApplicationName);
                return hash.ToHashCode();
            }
        }
    }
}
