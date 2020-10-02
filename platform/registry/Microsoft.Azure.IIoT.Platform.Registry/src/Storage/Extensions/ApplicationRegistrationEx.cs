// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Aapplication registration persisted and comparable
    /// </summary>
    public static class ApplicationRegistrationEx {

        /// <summary>
        /// Logical comparison of application registrations
        /// </summary>
        public static IEqualityComparer<ApplicationRegistration> Logical =>
            new LogicalEquality();

        /// <summary>
        /// Create device twin
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static DeviceTwinModel ToDeviceTwin(
            this ApplicationRegistration registration, IJsonSerializer serializer) {
            return Patch(null, registration, serializer);
        }

        /// <summary>
        /// Create patch twin model to upload
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="existing"></param>
        /// <param name="update"></param>
        public static DeviceTwinModel Patch(this ApplicationRegistration existing,
            ApplicationRegistration update, IJsonSerializer serializer) {
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }

            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();

            var applicationUri = existing?.ApplicationUri;
            var siteOrGatewayId = existing?.SiteOrGatewayId;
            var applicationType = existing?.ApplicationType;
            var hub = existing?.Hub;

            if (update != null) {
                // Tags

                if (update?.ApplicationId != null &&
                    update.ApplicationId != existing?.ApplicationId) {
                    tags.Add(nameof(ApplicationId), update.ApplicationId);
                }

                if (update?.IsDisabled != null &&
                    update.IsDisabled != existing?.IsDisabled) {
                    tags.Add(nameof(EntityRegistration.IsDisabled), (update?.IsDisabled ?? false) ?
                        true : (bool?)null);
                    tags.Add(nameof(EntityRegistration.NotSeenSince), (update?.IsDisabled ?? false) ?
                        DateTime.UtcNow : (DateTime?)null);
                }

                if (update?.SiteOrGatewayId != existing?.SiteOrGatewayId) {
                    tags.Add(nameof(ApplicationRegistration.SiteOrGatewayId), update?.SiteOrGatewayId);
                }

                if (update?.DiscovererId != existing?.DiscovererId) {
                    tags.Add(nameof(ApplicationRegistration.DiscovererId), update?.DiscovererId);
                }

                if (update?.SiteId != existing?.SiteId) {
                    tags.Add(nameof(ApplicationRegistration.SiteId), update?.SiteId);
                }

                if (update?.ApplicationType != null &&
                    update?.ApplicationType != existing?.ApplicationType) {
                    tags.Add(nameof(ApplicationRegistration.ApplicationType),
                        serializer.FromObject(update.ApplicationType.ToString()));
                    tags.Add(nameof(ApplicationType.Server),
                        update.ApplicationType != ApplicationType.Client);
                    tags.Add(nameof(ApplicationType.Client),
                        update.ApplicationType != ApplicationType.Server &&
                        update.ApplicationType != ApplicationType.DiscoveryServer);
                    tags.Add(nameof(ApplicationType.DiscoveryServer),
                        update.ApplicationType == ApplicationType.DiscoveryServer);
                }

                if (update?.ApplicationUri != existing?.ApplicationUri) {
                    tags.Add(nameof(ApplicationRegistration.ApplicationUri),
                        update?.ApplicationUri);
                    tags.Add(nameof(ApplicationRegistration.ApplicationUriUC),
                        update?.ApplicationUriUC);
                }

                if (update?.RecordId != existing?.RecordId) {
                    tags.Add(nameof(ApplicationRegistration.RecordId),
                        update?.RecordId);
                }

                if (update?.ApplicationName != existing?.ApplicationName) {
                    tags.Add(nameof(ApplicationRegistration.ApplicationName),
                        update?.ApplicationName);
                }

                if (update?.Locale != existing?.Locale) {
                    tags.Add(nameof(ApplicationRegistration.Locale),
                        update?.Locale);
                }

                if (update?.DiscoveryProfileUri != existing?.DiscoveryProfileUri) {
                    tags.Add(nameof(ApplicationRegistration.DiscoveryProfileUri),
                        update?.DiscoveryProfileUri);
                }

                if (update?.GatewayServerUri != existing?.GatewayServerUri) {
                    tags.Add(nameof(ApplicationRegistration.GatewayServerUri),
                        update?.GatewayServerUri);
                }

                if (update?.ProductUri != existing?.ProductUri) {
                    tags.Add(nameof(ApplicationRegistration.ProductUri), update?.ProductUri);
                }

                var urlUpdate = update?.DiscoveryUrls.DecodeAsList().SequenceEqualsSafe(
                    existing?.DiscoveryUrls?.DecodeAsList());
                if (!(urlUpdate ?? true)) {
                    tags.Add(nameof(ApplicationRegistration.DiscoveryUrls),
                        update?.DiscoveryUrls == null ?
                        null : serializer.FromObject(update.DiscoveryUrls));
                }

                var capsUpdate = update?.Capabilities.DecodeAsSet().SetEqualsSafe(
                    existing?.Capabilities?.DecodeAsSet());
                if (!(capsUpdate ?? true)) {
                    tags.Add(nameof(ApplicationRegistration.Capabilities),
                        update?.Capabilities == null ?
                        null : serializer.FromObject(update.Capabilities));
                }

                var namesUpdate = update?.LocalizedNames.DictionaryEqualsSafe(
                    existing?.LocalizedNames);
                if (!(namesUpdate ?? true)) {
                    tags.Add(nameof(ApplicationRegistration.LocalizedNames),
                        update?.LocalizedNames == null ?
                        null : serializer.FromObject(update.LocalizedNames));
                }

                var hostsUpdate = update?.HostAddresses.DecodeAsList().SequenceEqualsSafe(
                    existing?.HostAddresses?.DecodeAsList());
                if (!(hostsUpdate ?? true)) {
                    tags.Add(nameof(ApplicationRegistration.HostAddresses),
                        update?.HostAddresses == null ?
                        null : serializer.FromObject(update.HostAddresses));
                }

                if (update?.CreateAuthorityId != existing?.CreateAuthorityId) {
                    tags.Add(nameof(ApplicationRegistration.CreateAuthorityId),
                        update?.CreateAuthorityId);
                }
                if (update?.CreateTime != existing?.CreateTime) {
                    tags.Add(nameof(ApplicationRegistration.CreateTime),
                        update?.CreateTime);
                }

                if (update?.UpdateAuthorityId != existing?.UpdateAuthorityId) {
                    tags.Add(nameof(ApplicationRegistration.UpdateAuthorityId),
                        update?.UpdateAuthorityId);
                }
                if (update?.UpdateTime != existing?.UpdateTime) {
                    tags.Add(nameof(ApplicationRegistration.UpdateTime),
                        update?.UpdateTime);
                }

                // Recalculate identity

                if (update?.ApplicationUri != null) {
                    applicationUri = update?.ApplicationUri;
                }
                if (applicationUri == null) {
                    throw new ArgumentException(nameof(ApplicationRegistration.ApplicationUri));
                }

                if (siteOrGatewayId == null) {
                    siteOrGatewayId = update?.SiteOrGatewayId;
                    if (siteOrGatewayId == null) {
                        throw new ArgumentException(nameof(ApplicationRegistration.SiteOrGatewayId));
                    }
                }

                if (update?.ApplicationType != null) {
                    applicationType = update?.ApplicationType;
                }

                if (update?.Hub != null) {
                    hub = update?.Hub;
                }
            }

            tags.Add(nameof(EntityRegistration.DeviceType), update?.DeviceType);

            var twin = new DeviceTwinModel {
                Id = ApplicationInfoModelEx.CreateApplicationId(
                    siteOrGatewayId, applicationUri, applicationType),
                Etag = existing?.Etag,
                Hub = hub,
                Tags = tags,
                Properties = new TwinPropertiesModel {
                    Desired = properties
                }
            };

            if (existing?.DeviceId != twin.Id) {
                twin.Etag = null; // Force creation of new identity
            }
            return twin;
        }

        /// <summary>
        /// Patch registration
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="request"></param>
        public static void Patch(this ApplicationRegistration registration,
            ApplicationInfoUpdateModel request) {
            if (registration is null) {
                throw new ArgumentNullException(nameof(registration));
            }
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }
            // Patch
            if (!string.IsNullOrEmpty(request.ApplicationName)) {
                registration.ApplicationName = request.ApplicationName;
            }
            if (request.Capabilities != null) {
                registration.Capabilities =
                    request.Capabilities.EncodeSetAsDictionary();
            }
            if (!string.IsNullOrEmpty(request.DiscoveryProfileUri)) {
                registration.DiscoveryProfileUri = request.DiscoveryProfileUri;
            }
            if (!string.IsNullOrEmpty(request.GatewayServerUri)) {
                registration.GatewayServerUri = request.GatewayServerUri;
            }
            if (!string.IsNullOrEmpty(request.ProductUri)) {
                registration.ProductUri = request.ProductUri;
            }
            if (request.DiscoveryUrls != null) {
                registration.DiscoveryUrls =
                    request.DiscoveryUrls?.ToList().EncodeAsDictionary();
            }
            if (request.LocalizedNames != null) {
                var table = registration.LocalizedNames.Clone();
                foreach (var item in request.LocalizedNames) {
                    if (item.Value == null) {
                        table.Remove(item.Key);
                    }
                    else {
                        table.AddOrUpdate(item.Key, item.Value);
                    }
                }
                registration.LocalizedNames = table;
            }
            registration.Validate();
        }

        /// <summary>
        /// Validates all fields in an application record to be consistent with
        /// the OPC UA specification.
        /// </summary>
        /// <param name="registration">The application registration</param>
        public static void Validate(this ApplicationRegistration registration) {
            if (registration == null) {
                throw new ArgumentNullException(nameof(registration));
            }

            if (registration.ApplicationUri == null) {
                throw new ArgumentException("Application uri is bad",
                    nameof(registration));
            }

            if (!Uri.IsWellFormedUriString(registration.ApplicationUri, UriKind.Absolute)) {
                throw new ArgumentException(registration.ApplicationUri +
                    " is not a valid URI.", nameof(registration));
            }

            if ((registration.ApplicationType < ApplicationType.Server) ||
                (registration.ApplicationType > ApplicationType.DiscoveryServer)) {
                throw new ArgumentException(registration.ApplicationType.ToString() +
                    " is not a valid ApplicationType.", nameof(registration));
            }

            if (string.IsNullOrEmpty(registration.GetApplicationName())) {
                throw new ArgumentException(
                    "At least one ApplicationName must be provided.",
                    nameof(registration));
            }

            if (string.IsNullOrEmpty(registration.ProductUri)) {
                throw new ArgumentException(
                    "A ProductUri must be provided.", nameof(registration));
            }

            if (!Uri.IsWellFormedUriString(registration.ProductUri, UriKind.Absolute)) {
                throw new ArgumentException(registration.ProductUri +
                    " is not a valid URI.", nameof(registration));
            }

            if (registration.DiscoveryUrls != null) {
                foreach (var discoveryUrl in registration.DiscoveryUrls.DecodeAsList()) {
                    if (string.IsNullOrEmpty(discoveryUrl)) {
                        continue;
                    }
                    if (!Uri.IsWellFormedUriString(discoveryUrl, UriKind.Absolute)) {
                        throw new ArgumentException(discoveryUrl + " is not a valid URL.",
                            nameof(registration));
                    }
                    // TODO: check for https:/hostname:62541, typo is not detected here
                }
            }

            if (registration.ApplicationType != ApplicationType.Client) {
                if (!(registration.DiscoveryUrls.DecodeAsList()?.Any() ?? false)) {
                    throw new ArgumentException(
                        "At least one DiscoveryUrl must be provided.",
                        nameof(registration));
                }

                if (!registration.Capabilities.Any()) {
                    throw new ArgumentException(
                        "At least one Server Capability must be provided.",
                        nameof(registration));
                }

                // TODO: check for valid servercapabilities
            }
            else {
                if (registration.DiscoveryUrls.DecodeAsList()?.Any() ?? false) {
                    throw new ArgumentException(
                        "DiscoveryUrls must not be specified for clients.",
                        nameof(registration));
                }
            }
        }

        /// <summary>
        /// Make sure to get the registration information from the right place.
        /// Reported (truth) properties take precedence over desired.
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static ApplicationRegistration ToApplicationRegistration(this DeviceTwinModel twin) {
            if (twin == null) {
                return null;
            }
            var registration = new ApplicationRegistration {

                // Device

                Hub = twin.Hub,
                DeviceId = twin.Id,
                Etag = twin.Etag,

                // Tags

                IsDisabled =
                    twin.Tags.GetValueOrDefault(nameof(ApplicationRegistration.IsDisabled), twin.IsDisabled()),
                NotSeenSince =
                    twin.Tags.GetValueOrDefault<DateTime>(nameof(ApplicationRegistration.NotSeenSince), null),
                SiteId =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.SiteId), null),

                ApplicationName =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.ApplicationName), null),
                Locale =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.Locale), null),
                LocalizedNames =
                    twin.Tags.GetValueOrDefault<Dictionary<string, string>>(nameof(ApplicationRegistration.LocalizedNames), null),
                ApplicationUri =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.ApplicationUri), null),
                RecordId =
                    twin.Tags.GetValueOrDefault<uint>(nameof(ApplicationRegistration.RecordId), null),
                ProductUri =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.ProductUri), null),
                DiscovererId =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.DiscovererId),
                        twin.Tags.GetValueOrDefault<string>("SupervisorId", null)),
                DiscoveryProfileUri =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.DiscoveryProfileUri), null),
                GatewayServerUri =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.GatewayServerUri), null),
                ApplicationType =
                    twin.Tags.GetValueOrDefault<ApplicationType>(nameof(ApplicationType), null),
                Capabilities =
                    twin.Tags.GetValueOrDefault<Dictionary<string, bool>>(nameof(ApplicationRegistration.Capabilities), null),
                HostAddresses =
                    twin.Tags.GetValueOrDefault<Dictionary<string, string>>(nameof(ApplicationRegistration.HostAddresses), null),
                DiscoveryUrls =
                    twin.Tags.GetValueOrDefault<Dictionary<string, string>>(nameof(ApplicationRegistration.DiscoveryUrls), null),

                CreateTime =
                    twin.Tags.GetValueOrDefault<DateTime>(nameof(ApplicationRegistration.CreateTime), null),
                CreateAuthorityId =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.CreateAuthorityId), null),
                UpdateTime =
                    twin.Tags.GetValueOrDefault<DateTime>(nameof(ApplicationRegistration.UpdateTime), null),
                UpdateAuthorityId =
                    twin.Tags.GetValueOrDefault<string>(nameof(ApplicationRegistration.UpdateAuthorityId), null),
            };
            return registration;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static ApplicationRegistration Clone(this ApplicationRegistration registration) {
            if (registration == null) {
                return null;
            }
            return new ApplicationRegistration {
                DeviceId = registration.Id,
                Type = registration.Type,
                Etag = registration.Etag,
                IsDisabled = registration.IsDisabled,
                NotSeenSince = registration.NotSeenSince,
                SiteId = registration.SiteId,
                ApplicationName = registration.ApplicationName,
                Locale = registration.Locale,
                LocalizedNames = registration.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ApplicationUri = registration.ApplicationUri,
                RecordId = registration.RecordId,
                ProductUri = registration.ProductUri,
                DiscovererId = registration.DiscovererId,
                DiscoveryProfileUri = registration.DiscoveryProfileUri,
                GatewayServerUri = registration.GatewayServerUri,
                ApplicationType = registration.ApplicationType,
                Capabilities = registration.Capabilities?
                    .ToDictionary(k => k.Key, v => v.Value),
                HostAddresses = registration.HostAddresses?
                    .ToDictionary(k => k.Key, v => v.Value),
                DiscoveryUrls = registration.DiscoveryUrls?
                    .ToDictionary(k => k.Key, v => v.Value),
                CreateTime = registration.CreateTime,
                CreateAuthorityId = registration.CreateAuthorityId,
                UpdateTime = registration.UpdateTime,
                UpdateAuthorityId = registration.UpdateAuthorityId,
                Connected = registration.Connected,
            };
        }

        /// <summary>
        /// Decode tags and property into registration object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        /// <param name="etag"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static ApplicationRegistration ToApplicationRegistration(
            this ApplicationInfoModel model, bool? disabled = null, string etag = null,
            uint? recordId = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new ApplicationRegistration {
                IsDisabled = disabled,
                DiscovererId = model.DiscovererId,
                Etag = etag,
                RecordId = recordId,
                SiteId = model.SiteId,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames,
                HostAddresses = model.HostAddresses?.ToList().EncodeAsDictionary(),
                ApplicationType = model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ProductUri = model.ProductUri,
                NotSeenSince = model.NotSeenSince,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                GatewayServerUri = model.GatewayServerUri,
                Capabilities = model.Capabilities.EncodeSetAsDictionary(),
                DiscoveryUrls = model.DiscoveryUrls?.ToList().EncodeAsDictionary(),
                CreateAuthorityId = model.Created?.AuthorityId,
                CreateTime = model.Created?.Time,
                UpdateAuthorityId = model.Updated?.AuthorityId,
                UpdateTime = model.Updated?.Time,
                Version = null,
                Connected = false
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static ApplicationInfoModel ToServiceModel(this ApplicationRegistration registration) {
            if (registration == null) {
                return null;
            }
            return new ApplicationInfoModel {
                ApplicationId = registration.ApplicationId,
                ApplicationName = registration.ApplicationName,
                Locale = registration.Locale,
                LocalizedNames = registration.LocalizedNames,
                HostAddresses = registration.HostAddresses.DecodeAsList().ToHashSetSafe(),
                NotSeenSince = registration.NotSeenSince,
                ApplicationType = registration.ApplicationType ?? ApplicationType.Server,
                ApplicationUri = string.IsNullOrEmpty(registration.ApplicationUri) ?
                    registration.ApplicationUriUC : registration.ApplicationUri,
                ProductUri = registration.ProductUri,
                SiteId = string.IsNullOrEmpty(registration.SiteId) ?
                    null : registration.SiteId,
                DiscovererId = string.IsNullOrEmpty(registration.DiscovererId) ?
                    null : registration.DiscovererId,
                DiscoveryUrls = registration.DiscoveryUrls.DecodeAsList().ToHashSetSafe(),
                DiscoveryProfileUri = registration.DiscoveryProfileUri,
                GatewayServerUri = registration.GatewayServerUri,
                Capabilities = registration.Capabilities?.DecodeAsSet(),
                Created = ToOperationModel(registration.CreateAuthorityId, registration.CreateTime),
                Updated = ToOperationModel(registration.UpdateAuthorityId, registration.UpdateTime),
            };
        }

        /// <summary>
        /// Returns true if this registration matches the application
        /// model provided.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool Matches(this ApplicationRegistration registration,
            ApplicationInfoModel model) {
            if (registration == null) {
                return model == null;
            }
            return model != null &&
                registration.ApplicationId == model.ApplicationId &&
                registration.ApplicationType == model.ApplicationType &&
                registration.ApplicationUri == model.ApplicationUri &&
                registration.HostAddresses.DecodeAsList().SequenceEqualsSafe(
                    model.HostAddresses) &&
                registration.CreateAuthorityId == model.Created?.AuthorityId &&
                registration.UpdateAuthorityId == model.Updated?.AuthorityId &&
                registration.CreateTime == model.Created?.Time &&
                registration.UpdateTime == model.Updated?.Time &&
                registration.DiscoveryProfileUri == model.DiscoveryProfileUri &&
                registration.GatewayServerUri == model.GatewayServerUri &&
                registration.NotSeenSince == model.NotSeenSince &&
                registration.DiscovererId == model.DiscovererId &&
                registration.SiteId == model.SiteId &&
                registration.Capabilities.DecodeAsSet().SetEqualsSafe(
                    model.Capabilities?.Select(x =>
                        VariantValueEx.SanitizePropertyName(x).ToUpperInvariant())) &&
                registration.DiscoveryUrls.DecodeAsList().SequenceEqualsSafe(
                    model.DiscoveryUrls);
        }

        /// <summary>
        /// Returns application name
        /// </summary>
        /// <param name="registration">The application record.</param>
        public static string GetApplicationName(this ApplicationRegistration registration) {
            if (registration == null) {
                return null;
            }
            if (!string.IsNullOrEmpty(registration.ApplicationName)) {
                return registration.ApplicationName;
            }
            if (registration.LocalizedNames != null &&
                registration.LocalizedNames.Count != 0 &&
                !string.IsNullOrEmpty(registration.LocalizedNames.First().Value)) {
                return registration.LocalizedNames.First().Value;
            }
            return null;
        }

        /// <summary>
        /// Get site or gateway id from registration
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string GetSiteOrGatewayId(this ApplicationRegistration registration) {
            if (registration == null) {
                return null;
            }
            var siteOrGatewayId = registration?.SiteId;
            if (siteOrGatewayId == null) {
                var id = registration?.DiscovererId;
                if (id != null) {
                    siteOrGatewayId = HubResource.Parse(id, out _, out _);
                }
            }
            return siteOrGatewayId;
        }

        /// <summary>
        /// Create operation model
        /// </summary>
        /// <param name="authorityId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static RegistryOperationContextModel ToOperationModel(
            string authorityId, DateTime? time) {
            if (string.IsNullOrEmpty(authorityId) && time == null) {
                return null;
            }
            return new RegistryOperationContextModel {
                AuthorityId = authorityId,
                Time = time ?? DateTime.MinValue
            };
        }

        /// <summary>
        /// Compares for logical equality - applications are logically equivalent if they
        /// have the same uri, type, and site location or supervisor that registered.
        /// </summary>
        private class LogicalEquality : IEqualityComparer<ApplicationRegistration> {

            /// <inheritdoc />
            public bool Equals(ApplicationRegistration x, ApplicationRegistration y) {
                return
                    x.SiteOrGatewayId == y.SiteOrGatewayId &&
                    x.ApplicationType == y.ApplicationType &&
                    x.ApplicationUriUC == y.ApplicationUriUC;
            }

            /// <inheritdoc />
            public int GetHashCode(ApplicationRegistration obj) {
                var hashCode = 1200389859;
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<ApplicationType?>.Default.GetHashCode(obj.ApplicationType);
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string>.Default.GetHashCode(obj.ApplicationUriUC);
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string>.Default.GetHashCode(obj.SiteOrGatewayId);
                return hashCode;
            }
        }
    }
}
