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
        /// Create unique application id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string CreateApplicationId(ApplicationInfoModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            var siteOrGatewayId = model.SiteId;
            if (siteOrGatewayId == null && model.DiscovererId != null) {
                siteOrGatewayId = HubResource.Parse(model.DiscovererId, out _, out _);
            }
            return CreateApplicationId(siteOrGatewayId, model.ApplicationUri,
                model.ApplicationType);
        }

        /// <summary>
        /// Create unique application id
        /// </summary>
        /// <param name="siteOrGatewayId"></param>
        /// <param name="applicationUri"></param>
        /// <param name="applicationType"></param>
        /// <returns></returns>
        public static string CreateApplicationId(string siteOrGatewayId,
            string applicationUri, ApplicationType? applicationType) {
            if (string.IsNullOrEmpty(applicationUri)) {
                return null;
            }
            applicationUri = applicationUri.ToLowerInvariant();
            var type = applicationType ?? ApplicationType.Server;
            var id = $"{siteOrGatewayId ?? ""}-{type}-{applicationUri}";
            var prefix = applicationType == ApplicationType.Client ? "uac" : "uas";
            return prefix + id.ToSha1Hash();
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
        public static bool IsSameAs(this ApplicationInfoModel model,
            ApplicationInfoModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            return
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
                ProductUri = model.ProductUri,
                SiteId = model.SiteId,
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
            this ApplicationInfoModel model, RegistryOperationContextModel context = null) {
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
                SiteId = model.SiteId,
                Context = context
            };
        }

        /// <summary>
        /// Convert registration request to application info model
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="disabled"></param>
        /// <returns></returns>
        public static ApplicationInfoModel ToApplicationInfo(
            this ApplicationRegistrationRequestModel request,
            RegistryOperationContextModel context,
            bool disabled = true) {
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
                SiteId = request.SiteId,
                Created = context,
                NotSeenSince = disabled ? DateTime.UtcNow : (DateTime?)null,
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
            this ApplicationInfoModel model, RegistryOperationContextModel context = null) {
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
            application.ApplicationId = model.ApplicationId;
            application.ApplicationName = model.ApplicationName;
            application.LocalizedNames = model.LocalizedNames;
            application.ApplicationType = model.ApplicationType;
            application.ApplicationUri = model.ApplicationUri;
            application.Capabilities = model.Capabilities;
            application.DiscoveryProfileUri = model.DiscoveryProfileUri;
            application.HostAddresses = model.HostAddresses;
            application.DiscoveryUrls = model.DiscoveryUrls;
            application.NotSeenSince = model.NotSeenSince;
            application.ProductUri = model.ProductUri;
            application.SiteId = model.SiteId;
            application.DiscovererId = model.DiscovererId;
            application.GatewayServerUri = model.GatewayServerUri;
            application.Created = model.Created;
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
            if (!string.IsNullOrWhiteSpace(model.ApplicationName)) {
                return model.ApplicationName;
            }
            return model.LocalizedNames?
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n.Value)).Value;
        }

        /// <summary>
        /// Returns the site or supervisor id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetSiteOrGatewayId(this ApplicationInfoModel model) {
            if (string.IsNullOrEmpty(model.SiteId)) {
                return model.DiscovererId;
            }
            return model.SiteId;
        }

        /// <summary>
        /// Compares for logical equality - applications are logically equivalent if they
        /// have the same uri, type, and site location or supervisor that registered.
        /// </summary>
        private class LogicalComparer : IEqualityComparer<ApplicationInfoModel> {

            /// <inheritdoc />
            public bool Equals(ApplicationInfoModel x, ApplicationInfoModel y) {
                if (x.GetSiteOrGatewayId() != y.GetSiteOrGatewayId()) {
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
                hash.Add(obj.ApplicationUri?.ToLowerInvariant());
                hash.Add(obj.GetSiteOrGatewayId());
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
                    x.GetSiteOrGatewayId() == y.GetSiteOrGatewayId() &&
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
                hash.Add(obj.ApplicationType);
                hash.Add(obj.ApplicationUri?.ToLowerInvariant());
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
