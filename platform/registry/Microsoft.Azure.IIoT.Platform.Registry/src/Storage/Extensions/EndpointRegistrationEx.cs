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
    /// Twin (endpoint) registration extensions
    /// </summary>
    public static class EndpointRegistrationEx {
        /// <summary>
        /// Create device twin
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static DeviceTwinModel ToDeviceTwin(this EndpointRegistration registration,
            IJsonSerializer serializer) {
            return Patch(null, registration, serializer);
        }

        /// <summary>
        /// Create patch twin model to upload
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="update"></param>
        /// <param name="serializer"></param>
        public static DeviceTwinModel Patch(this EndpointRegistration existing,
            EndpointRegistration update, IJsonSerializer serializer) {
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }
            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();

            var reportedEndpointUrl = existing?.EndpointRegistrationUrl;
            var applicationId = existing?.ApplicationId;
            var securityMode = existing?.SecurityMode;
            var securityPolicy = existing?.SecurityPolicy;

            if (update != null) {

                // Tags

                if (update.ApplicationId != null &&
                    update.ApplicationId != existing?.ApplicationId) {
                    tags.Add(nameof(ApplicationId), update.ApplicationId);
                }

                if (update.IsDisabled != null &&
                    update.IsDisabled != existing?.IsDisabled) {
                    tags.Add(nameof(EntityRegistration.IsDisabled), (update.IsDisabled ?? false) ?
                        true : (bool?)null);
                    tags.Add(nameof(EntityRegistration.NotSeenSince), (update.IsDisabled ?? false) ?
                        DateTime.UtcNow : (DateTime?)null);
                }

                if (update.SiteOrGatewayId != existing?.SiteOrGatewayId) {
                    tags.Add(nameof(EndpointRegistration.SiteOrGatewayId), update.SiteOrGatewayId);
                }

                if (update.SupervisorId != existing?.SupervisorId) {
                    tags.Add(nameof(EndpointRegistration.SupervisorId), update.SupervisorId);
                }

                if (update.DiscovererId != existing?.DiscovererId) {
                    tags.Add(nameof(EndpointRegistration.DiscovererId), update.DiscovererId);
                }

                if (update.SiteId != existing?.SiteId) {
                    tags.Add(nameof(EndpointRegistration.SiteId), update.SiteId);
                }

                if (update.EndpointRegistrationUrl != null &&
                    update.EndpointRegistrationUrl != existing?.EndpointRegistrationUrl) {
                    tags.Add(nameof(EndpointRegistration.EndpointUrlLC),
                        update.EndpointUrlLC);
                    tags.Add(nameof(EndpointRegistration.EndpointRegistrationUrl),
                        update.EndpointRegistrationUrl);
                }

                if (update.SecurityLevel != existing?.SecurityLevel) {
                    tags.Add(nameof(EndpointRegistration.SecurityLevel), update.SecurityLevel == null ?
                        null : serializer.FromObject(update.SecurityLevel.ToString()));
                }

                if (update.Activated != null &&
                    update.Activated != existing?.Activated) {
                    tags.Add(nameof(EndpointRegistration.Activated), update.Activated);
                }

                var methodEqual = update.AuthenticationMethods.DecodeAsList().SetEqualsSafe(
                    existing?.AuthenticationMethods?.DecodeAsList(), VariantValue.DeepEquals);
                if (methodEqual) {
                    tags.Add(nameof(EndpointRegistration.AuthenticationMethods),
                        update.AuthenticationMethods == null ?
                        null : serializer.FromObject(update.AuthenticationMethods));
                }

                // Properties

                if (update.EndpointUrl != null &&
                    update.EndpointUrl != existing?.EndpointUrl) {
                    properties.Add(nameof(EndpointRegistration.EndpointUrl),
                        update.EndpointUrl);
                }

                var urlsEqual = update.AlternativeUrls.DecodeAsList().ToHashSetSafe().SetEqualsSafe(
                    existing?.AlternativeUrls?.DecodeAsList());
                if (urlsEqual) {
                    properties.Add(nameof(EndpointRegistration.AlternativeUrls),
                        update.AlternativeUrls == null ?
                        null : serializer.FromObject(update.AlternativeUrls));
                }

                if (update.SecurityMode != null &&
                    update.SecurityMode != existing?.SecurityMode) {
                    properties.Add(nameof(EndpointRegistration.SecurityMode),
                        update.SecurityMode == null ?
                            null : serializer.FromObject(update.SecurityMode.ToString()));
                }

                if (update.SecurityPolicy != null &&
                    update.SecurityPolicy != existing?.SecurityPolicy) {
                    properties.Add(nameof(EndpointRegistration.SecurityPolicy),
                        update.SecurityPolicy);
                }

                if (update.Thumbprint != existing?.Thumbprint) {
                    properties.Add(nameof(EndpointRegistration.Thumbprint), update.Thumbprint);
                }

                // To recalculate identity

                if (update.EndpointRegistrationUrl != null) {
                    reportedEndpointUrl = update.EndpointRegistrationUrl;
                }
                if (reportedEndpointUrl == null) {
                    throw new ArgumentException(nameof(EndpointRegistration.EndpointUrl));
                }
                if (update.ApplicationId != null) {
                    applicationId = update.ApplicationId;
                }
                if (applicationId == null) {
                    throw new ArgumentException(nameof(EndpointRegistration.ApplicationId));
                }
                if (update.SecurityMode != null) {
                    securityMode = update.SecurityMode;
                }
                if (update.SecurityPolicy != null) {
                    securityPolicy = update.SecurityPolicy;
                }
            }

            tags.Add(nameof(EntityRegistration.DeviceType), update?.DeviceType);

            var twin = new DeviceTwinModel {
                Id = EndpointInfoModelEx.CreateEndpointId(
                    applicationId, reportedEndpointUrl, securityMode, securityPolicy),
                Etag = existing?.Etag,
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
        /// Decode tags and property into registration object
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static EndpointRegistration ToEndpointRegistration(this DeviceTwinModel twin,
            Dictionary<string, VariantValue> properties) {
            if (twin == null) {
                return null;
            }

            var registration = new EndpointRegistration {
                // Device

                Hub = twin.Hub,
                DeviceId = twin.Id,
                Etag = twin.Etag,
                Version = null,
                Connected = twin.IsConnected() ?? false,

                // Tags
                IsDisabled =
                    twin.Tags.GetValueOrDefault(nameof(EndpointRegistration.IsDisabled), twin.IsDisabled()),
                NotSeenSince =
                    twin.Tags.GetValueOrDefault<DateTime>(nameof(EndpointRegistration.NotSeenSince), null),

                SupervisorId =
                    twin.Tags.GetValueOrDefault<string>(nameof(EndpointRegistration.SupervisorId), null),
                DiscovererId =
                    twin.Tags.GetValueOrDefault(nameof(EndpointRegistration.DiscovererId),
                        twin.Tags.GetValueOrDefault<string>(nameof(EndpointRegistration.SupervisorId), null)),
                Activated =
                    twin.Tags.GetValueOrDefault<bool>(nameof(EndpointRegistration.Activated), null),
                ApplicationId =
                    twin.Tags.GetValueOrDefault<string>(nameof(EndpointRegistration.ApplicationId), null),
                SecurityLevel =
                    twin.Tags.GetValueOrDefault<int>(nameof(EndpointRegistration.SecurityLevel), null),
                AuthenticationMethods =
                    twin.Tags.GetValueOrDefault<Dictionary<string, VariantValue>>(
                        nameof(EndpointRegistration.AuthenticationMethods), null),
                EndpointRegistrationUrl =
                    twin.Tags.GetValueOrDefault<string>(nameof(EndpointRegistration.EndpointRegistrationUrl), null),
                SiteId =
                    twin.Tags.GetValueOrDefault<string>(nameof(EndpointRegistration.SiteId), null),

                // Properties

                Type =
                    properties.GetValueOrDefault<string>(TwinProperty.Type, null),
                State =
                    properties.GetValueOrDefault(nameof(EndpointRegistration.State),
                    EndpointConnectivityState.Disconnected),
                EndpointUrl =
                    properties.GetValueOrDefault<string>(nameof(EndpointRegistration.EndpointUrl), null),
                AlternativeUrls =
                    properties.GetValueOrDefault<Dictionary<string, string>>(nameof(EndpointRegistration.AlternativeUrls), null),
                SecurityMode =
                    properties.GetValueOrDefault<SecurityMode>(nameof(EndpointRegistration.SecurityMode), null),
                SecurityPolicy =
                    properties.GetValueOrDefault<string>(nameof(EndpointRegistration.SecurityPolicy), null),
                Thumbprint =
                    properties.GetValueOrDefault<string>(nameof(EndpointRegistration.Thumbprint), null)
            };
            return registration;
        }

        /// <summary>
        /// Make sure to get the registration information from the right place.
        /// Reported (truth) properties take precedence over desired. However,
        /// if there is nothing reported, it means the endpoint is not currently
        /// serviced, thus we use desired as if they are attributes of the
        /// endpoint.
        /// </summary>
        /// <param name="twin"></param>
        /// <returns></returns>
        public static EndpointRegistration ToEndpointRegistration(this DeviceTwinModel twin) {
            if (twin == null) {
                return null;
            }
            if (twin.Tags == null) {
                twin.Tags = new Dictionary<string, VariantValue>();
            }
            var consolidated =
                ToEndpointRegistration(twin, twin.GetConsolidatedProperties());
            return consolidated;
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static EndpointInfoModel ToServiceModel(this EndpointRegistration registration, string etag) {
            if (registration == null) {
                return null;
            }
            return new EndpointInfoModel {
                ApplicationId = registration.ApplicationId,
                GenerationId = etag,
                Id = registration.DeviceId,
                SiteId = string.IsNullOrEmpty(registration.SiteId) ?
                    null : registration.SiteId,
                SupervisorId = string.IsNullOrEmpty(registration.SupervisorId) ?
                    null : registration.SupervisorId,
                DiscovererId = string.IsNullOrEmpty(registration.DiscovererId) ?
                    null : registration.DiscovererId,
                AuthenticationMethods = registration.AuthenticationMethods?.DecodeAsList(j =>
                    j.ConvertTo<AuthenticationMethodModel>()),
                SecurityLevel = registration.SecurityLevel,
                EndpointUrl = string.IsNullOrEmpty(registration.EndpointRegistrationUrl) ?
                    (string.IsNullOrEmpty(registration.EndpointUrl) ?
                        registration.EndpointUrlLC : registration.EndpointUrl) : registration.EndpointRegistrationUrl,
                Endpoint = new EndpointModel {
                    Url = string.IsNullOrEmpty(registration.EndpointUrl) ?
                        registration.EndpointUrlLC : registration.EndpointUrl,
                    AlternativeUrls = registration.AlternativeUrls?.DecodeAsList().ToHashSetSafe(),
                    SecurityMode = registration.SecurityMode == SecurityMode.Best ?
                        null : registration.SecurityMode,
                    SecurityPolicy = string.IsNullOrEmpty(registration.SecurityPolicy) ?
                        null : registration.SecurityPolicy,
                    Certificate = registration.Thumbprint
                },
                ActivationState = registration.ActivationState,
                NotSeenSince = registration.NotSeenSince,
                EndpointState = registration.ActivationState == EntityActivationState.ActivatedAndConnected ?
                    (registration.State == EndpointConnectivityState.Disconnected ?
                        EndpointConnectivityState.Connecting : registration.State) :
                            EndpointConnectivityState.Disconnected
            };
        }

        /// <summary>
        /// Decode tags and property into registration object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="serializer"></param>
        /// <param name="disabled"></param>
        /// <param name="discoverId"></param>
        /// <param name="supervisorId"></param>
        /// <param name="applicationId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static EndpointRegistration ToEndpointRegistration(this EndpointInfoModel model,
            IJsonSerializer serializer, bool? disabled = null, string discoverId = null,
            string supervisorId = null, string applicationId = null, string siteId = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }

            return new EndpointRegistration {
                IsDisabled = disabled,
                NotSeenSince = model.NotSeenSince,
                ApplicationId = applicationId ?? model.ApplicationId,
                SiteId = siteId ?? model.SiteId,
                SupervisorId = supervisorId ?? model.SupervisorId,
                DiscovererId = discoverId ?? model.DiscovererId,
                SecurityLevel = model.SecurityLevel,
                EndpointRegistrationUrl = model.EndpointUrl ??
                    model.Endpoint.Url,
                EndpointUrl = model.Endpoint.Url,
                AlternativeUrls = model.Endpoint.AlternativeUrls?.ToList()?
                    .EncodeAsDictionary(),
                AuthenticationMethods = model.AuthenticationMethods?
                    .EncodeAsDictionary(serializer.FromObject),
                SecurityMode = model.Endpoint.SecurityMode ??
                    SecurityMode.Best,
                SecurityPolicy = model.Endpoint.SecurityPolicy,
                Thumbprint = model.Endpoint.Certificate,
                ActivationState = model.ActivationState
            };
        }

        /// <summary>
        /// Get site or gateway id from registration
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string GetSiteOrGatewayId(this EndpointRegistration registration) {
            if (registration == null) {
                return null;
            }
            var siteOrGatewayId = registration?.SiteId;
            if (siteOrGatewayId == null) {
                var id = registration?.DiscovererId ?? registration?.SupervisorId;
                if (id != null) {
                    siteOrGatewayId = HubResource.Parse(id, out _, out _);
                }
            }
            return siteOrGatewayId;
        }
    }
}
