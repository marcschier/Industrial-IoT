// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
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
        public static IEqualityComparer<ApplicationInfoModel> LogicalEquality { get; } =
            new LogicalComparer();

        /// <summary>
        /// Get structural equality comparer
        /// </summary>
        public static IEqualityComparer<ApplicationInfoModel> StructuralEquality { get; } =
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
        public static bool IsNotSeen(this ApplicationInfoModel model) {
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
        public static ApplicationInfoModel SetNotSeen(this ApplicationInfoModel model) {
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
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationRegistrationRequestModel ToRegistrationRequest(
            this ApplicationInfoModel model, OperationContextModel context = null) {
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
                Context = context
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
        /// Convert to Update model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationInfoUpdateModel ToUpdateRequest(
            this ApplicationInfoModel model, OperationContextModel context = null) {
            if (model is null) {
                return null;
            }
            return new ApplicationInfoUpdateModel {
                ApplicationName = model.ApplicationName,
                Capabilities = model.Capabilities,
                GenerationId = model.GenerationId,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                LocalizedNames = model.LocalizedNames,
                ProductUri = model.ProductUri,
                Locale = model.Locale,
                Context = context
            };
        }

        /// <summary>
        /// Patch application
        /// </summary>
        /// <param name="application"></param>
        /// <param name="model"></param>
        public static ApplicationInfoModel Patch(this ApplicationInfoModel application,
            ApplicationInfoModel model) {
            if (model is null) {
                return application;
            }
            if (application is null) {
                return model;
            }
            application.ApplicationId = model.ApplicationId;
            application.ApplicationName = model.ApplicationName;
            application.LocalizedNames = model.LocalizedNames;
            application.ApplicationType = model.ApplicationType;
            application.ApplicationUri = model.ApplicationUri;
            application.Capabilities = model.Capabilities;
            application.DiscoveryProfileUri = model.DiscoveryProfileUri;
            application.HostAddresses = model.HostAddresses;
            application.DiscoveryUrls = model.DiscoveryUrls;
            application.Visibility = model.Visibility;
            application.NotSeenSince = model.NotSeenSince;
            application.ProductUri = model.ProductUri;
            application.DiscovererId = model.DiscovererId;
            application.GatewayServerUri = model.GatewayServerUri;
            application.Updated = model.Updated;
            application.Locale = model.Locale;
            return application;
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
                return
                    x.DiscovererId == y.DiscovererId &&
                    x.ApplicationType == y.ApplicationType &&
                    x.ApplicationUri.EqualsIgnoreCase(y.ApplicationUri) &&
                    x.DiscoveryProfileUri == y.DiscoveryProfileUri &&
                    x.GatewayServerUri == y.GatewayServerUri &&
                    x.ProductUri == y.ProductUri &&
                    x.Locale == y.Locale &&
                    x.HostAddresses.SetEqualsSafe(y.HostAddresses) &&
                    x.ApplicationName == y.ApplicationName &&
                    x.LocalizedNames.DictionaryEqualsSafe(y.LocalizedNames) &&
                    x.Capabilities.SetEqualsSafe(y.Capabilities) &&
                    x.DiscoveryUrls.SetEqualsSafe(y.DiscoveryUrls);
            }

            /// <inheritdoc />
            public int GetHashCode(ApplicationInfoModel obj) {
                var hash = new HashCode();
                hash.Add(obj.DiscovererId);
                hash.Add(obj.ApplicationType);
                hash.Add(obj.ApplicationUri?.ToUpperInvariant());
                hash.Add(obj.ProductUri);
                hash.Add(obj.Locale);
                hash.Add(obj.DiscoveryProfileUri);
                hash.Add(obj.GatewayServerUri);
                hash.Add(obj.ApplicationName);
                return hash.ToHashCode();
            }
        }
    }
}
