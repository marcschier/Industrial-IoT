// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Model conversion extensions
    /// </summary>
    public static class ModelExtensions {

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoApiModel ToApiModel(
            this ApplicationInfoModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoApiModel {
                ApplicationId = model.ApplicationId,
                GenerationId = model.GenerationId,
                ApplicationType = (Core.Api.Models.ApplicationType)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames,
                ProductUri = model.ProductUri,
                HostAddresses = model.HostAddresses,
                DiscovererId = model.DiscovererId,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                Capabilities = model.Capabilities,
                NotSeenSince = model.NotSeenSince,
                Visibility = (EntityVisibility?)model.Visibility,
                GatewayServerUri = model.GatewayServerUri,
                Created = model.Created.ToApiModel(),
                Updated = model.Updated.ToApiModel(),
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoModel ToServiceModel(
            this ApplicationInfoApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoModel {
                ApplicationId = model.ApplicationId,
                GenerationId = model.GenerationId,
                ApplicationType = (Core.Models.ApplicationType)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames,
                ProductUri = model.ProductUri,
                HostAddresses = model.HostAddresses,
                DiscovererId = model.DiscovererId,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                Capabilities = model.Capabilities,
                NotSeenSince = model.NotSeenSince,
                Visibility = (Discovery.Models.EntityVisibility?)model.Visibility,
                GatewayServerUri = model.GatewayServerUri,
                Created = model.Created.ToServiceModel(),
                Updated = model.Updated.ToServiceModel(),
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoListApiModel ToApiModel(
            this ApplicationInfoListModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoListApiModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoListModel ToServiceModel(
            this ApplicationInfoListApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationApiModel ToApiModel(
            this ApplicationRegistrationModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationApiModel {
                Application = model.Application.ToApiModel(),
                Endpoints = model.Endpoints?
                    .Select(e => e.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationModel ToServiceModel(
            this ApplicationRegistrationApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationModel {
                Application = model.Application.ToServiceModel(),
                Endpoints = model.Endpoints?
                    .Select(e => e.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoQueryApiModel ToApiModel(
            this ApplicationInfoQueryModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoQueryApiModel {
                ApplicationType = (Core.Api.Models.ApplicationType?)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ProductUri = model.ProductUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                Capability = model.Capability,
                Visibility = (EntityVisibility?)model.Visibility,
                GatewayServerUri = model.GatewayServerUri,
                DiscovererId = model.DiscovererId,
                DiscoveryProfileUri = model.DiscoveryProfileUri
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoQueryModel ToServiceModel(
            this ApplicationInfoQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoQueryModel {
                ApplicationType = (Core.Models.ApplicationType?)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ProductUri = model.ProductUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                Capability = model.Capability,
                Visibility = (Discovery.Models.EntityVisibility?)model.Visibility,
                GatewayServerUri = model.GatewayServerUri,
                DiscovererId = model.DiscovererId,
                DiscoveryProfileUri = model.DiscoveryProfileUri
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationRequestApiModel ToApiModel(
            this ApplicationRegistrationRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationRequestApiModel {
                ApplicationType = (Core.Api.Models.ApplicationType?)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ProductUri = model.ProductUri,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                Capabilities = model.Capabilities
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationRequestModel ToServiceModel(
            this ApplicationRegistrationRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationRequestModel {
                ApplicationType = (Core.Models.ApplicationType?)model.ApplicationType,
                ApplicationUri = model.ApplicationUri,
                ApplicationName = model.ApplicationName,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ProductUri = model.ProductUri,
                DiscoveryProfileUri = model.DiscoveryProfileUri,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                Capabilities = model.Capabilities
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationResponseApiModel ToApiModel(
            this ApplicationRegistrationResultModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationResponseApiModel {
                Id = model.Id
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationRegistrationResultModel ToServiceModel(
            this ApplicationRegistrationResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationRegistrationResultModel {
                Id = model.Id
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoUpdateApiModel ToApiModel(
            this ApplicationInfoUpdateModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoUpdateApiModel {
                ApplicationName = model.ApplicationName,
                GenerationId = model.GenerationId,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ProductUri = model.ProductUri,
                Capabilities = model.Capabilities,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                DiscoveryProfileUri = model.DiscoveryProfileUri
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationInfoUpdateModel ToServiceModel(
            this ApplicationInfoUpdateApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationInfoUpdateModel {
                ApplicationName = model.ApplicationName,
                GenerationId = model.GenerationId,
                Locale = model.Locale,
                LocalizedNames = model.LocalizedNames?
                    .ToDictionary(k => k.Key, v => v.Value),
                ProductUri = model.ProductUri,
                Capabilities = model.Capabilities,
                DiscoveryUrls = model.DiscoveryUrls,
                GatewayServerUri = model.GatewayServerUri,
                DiscoveryProfileUri = model.DiscoveryProfileUri
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationSiteListApiModel ToApiModel(
            this ApplicationSiteListModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationSiteListApiModel {
                ContinuationToken = model.ContinuationToken,
                Sites = model.Sites?.ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationSiteListModel ToServiceModel(
            this ApplicationSiteListApiModel model) {
            if (model == null) {
                return null;
            }
            return new ApplicationSiteListModel {
                ContinuationToken = model.ContinuationToken,
                Sites = model.Sites?.ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationMethodApiModel ToApiModel(
            this AuthenticationMethodModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMethodApiModel {
                Id = model.Id,
                SecurityPolicy = model.SecurityPolicy,
                Configuration = model.Configuration,
                CredentialType = (Core.Api.Models.CredentialType)model.CredentialType
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AuthenticationMethodModel ToServiceModel(
            this AuthenticationMethodApiModel model) {
            if (model == null) {
                return null;
            }
            return new AuthenticationMethodModel {
                Id = model.Id,
                SecurityPolicy = model.SecurityPolicy,
                Configuration = model.Configuration,
                CredentialType = (Core.Models.CredentialType)model.CredentialType
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
                DiscoveryUrls = model.DiscoveryUrls
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
                AddressRangesToScan = model.AddressRangesToScan,
                NetworkProbeTimeout = model.NetworkProbeTimeout,
                MaxNetworkProbes = model.MaxNetworkProbes,
                PortRangesToScan = model.PortRangesToScan,
                PortProbeTimeout = model.PortProbeTimeout,
                MaxPortProbes = model.MaxPortProbes,
                MinPortProbesPercent = model.MinPortProbesPercent,
                IdleTimeBetweenScans = model.IdleTimeBetweenScans,
                Locales = model.Locales,
                DiscoveryUrls = model.DiscoveryUrls
            };
        }

        /// <summary>
        /// Convert to Api model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryCancelApiModel ToApiModel(
            this DiscoveryCancelModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryCancelApiModel {
                Id = model.Id
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryCancelModel ToServiceModel(
            this DiscoveryCancelApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryCancelModel {
                Id = model.Id
            };
        }

        /// <summary>
        /// Convert to Api model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryRequestApiModel ToApiModel(
            this DiscoveryRequestModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryRequestApiModel {
                Id = model.Id,
                Configuration = model.Configuration.ToApiModel(),
                Discovery = (DiscoveryMode?)model.Discovery
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscoveryRequestModel ToServiceModel(
            this DiscoveryRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscoveryRequestModel {
                Id = model.Id,
                Configuration = model.Configuration.ToServiceModel(),
                Discovery = (Discovery.Models.DiscoveryMode?)model.Discovery
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointApiModel ToApiModel(
            this EndpointModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointApiModel {
                Url = model.Url,
                AlternativeUrls = model.AlternativeUrls,
                SecurityMode = (Core.Api.Models.SecurityMode?)model.SecurityMode,
                SecurityPolicy = model.SecurityPolicy,
                Certificate = model.Certificate,
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointModel ToServiceModel(
            this EndpointApiModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointModel {
                Url = model.Url,
                AlternativeUrls = model.AlternativeUrls,
                SecurityMode = (Core.Models.SecurityMode?)model.SecurityMode,
                SecurityPolicy = model.SecurityPolicy,
                Certificate = model.Certificate,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoApiModel ToApiModel(
            this EndpointInfoModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoApiModel {
                ApplicationId = model.ApplicationId,
                GenerationId = model.GenerationId,
                NotSeenSince = model.NotSeenSince,
                Visibility = (EntityVisibility?)model.Visibility,
                Id = model.Id,
                Endpoint = model.Endpoint.ToApiModel(),
                AuthenticationMethods = model.AuthenticationMethods?
                    .Select(p => p.ToApiModel())
                    .ToList(),
                SecurityLevel = model.SecurityLevel,
                DiscovererId = model.DiscovererId,
                Created = model.Created.ToApiModel(),
                Updated = model.Updated.ToApiModel(),
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoModel ToServiceModel(
            this EndpointInfoApiModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoModel {
                ApplicationId = model.ApplicationId,
                GenerationId = model.GenerationId,
                NotSeenSince = model.NotSeenSince,
                Visibility = (Discovery.Models.EntityVisibility?)model.Visibility,
                Id = model.Id,
                Endpoint = model.Endpoint.ToServiceModel(),
                AuthenticationMethods = model.AuthenticationMethods?
                    .Select(p => p.ToServiceModel())
                    .ToList(),
                SecurityLevel = model.SecurityLevel,
                DiscovererId = model.DiscovererId,
                Created = model.Created.ToServiceModel(),
                Updated = model.Updated.ToServiceModel(),
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoListApiModel ToApiModel(
            this EndpointInfoListModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoListApiModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoListModel ToServiceModel(
            this EndpointInfoListApiModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert to Api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoQueryApiModel ToApiModel(
            this EndpointInfoQueryModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoQueryApiModel {
                Url = model.Url,
                Certificate = model.Certificate,
                SecurityPolicy = model.SecurityPolicy,
                SecurityMode = (Core.Api.Models.SecurityMode?)model.SecurityMode,
                ApplicationId = model.ApplicationId,
                DiscovererId = model.DiscovererId,
                Visibility = (EntityVisibility?)model.Visibility,
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointInfoQueryModel ToServiceModel(
            this EndpointInfoQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointInfoQueryModel {
                Url = model.Url,
                Certificate = model.Certificate,
                SecurityPolicy = model.SecurityPolicy,
                SecurityMode = (Core.Models.SecurityMode?)model.SecurityMode,
                ApplicationId = model.ApplicationId,
                DiscovererId = model.DiscovererId,
                Visibility = (Discovery.Models.EntityVisibility?)model.Visibility,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static OperationContextApiModel ToApiModel(
            this OperationContextModel model) {
            if (model == null) {
                return null;
            }
            return new OperationContextApiModel {
                Time = model.Time,
                AuthorityId = model.AuthorityId
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static OperationContextModel ToServiceModel(
            this OperationContextApiModel model) {
            if (model == null) {
                return null;
            }
            return new OperationContextModel {
                Time = model.Time,
                AuthorityId = model.AuthorityId
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ServerRegistrationRequestApiModel ToApiModel(
            this ServerRegistrationRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ServerRegistrationRequestApiModel {
                DiscoveryUrl = model.DiscoveryUrl,
                Id = model.Id,
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ServerRegistrationRequestModel ToServiceModel(
            this ServerRegistrationRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ServerRegistrationRequestModel {
                DiscoveryUrl = model.DiscoveryUrl,
                Id = model.Id,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        public static X509CertificateApiModel ToApiModel(
            this X509CertificateModel model) {
            if (model == null) {
                return null;
            }
            return new X509CertificateApiModel {
                Certificate = model.Certificate,
                NotAfterUtc = model.NotAfterUtc,
                NotBeforeUtc = model.NotBeforeUtc,
                SerialNumber = model.SerialNumber,
                Subject = model.Subject,
                SelfSigned = model.SelfSigned,
                Thumbprint = model.Thumbprint
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        public static X509CertificateModel ToServiceModel(
            this X509CertificateApiModel model) {
            if (model == null) {
                return null;
            }
            return new X509CertificateModel {
                Certificate = model.Certificate,
                NotAfterUtc = model.NotAfterUtc,
                NotBeforeUtc = model.NotBeforeUtc,
                SerialNumber = model.SerialNumber,
                Subject = model.Subject,
                SelfSigned = model.SelfSigned,
                Thumbprint = model.Thumbprint
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        public static X509CertificateChainApiModel ToApiModel(
            this X509CertificateChainModel model) {
            if (model == null) {
                return null;
            }
            return new X509CertificateChainApiModel {
                Status = model.Status?
                    .Select(s => (Core.Api.Models.X509ChainStatus)s)
                    .ToList(),
                Chain = model.Chain?
                    .Select(c => c.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        public static X509CertificateChainModel ToServiceModel(
            this X509CertificateChainApiModel model) {
            if (model == null) {
                return null;
            }
            return new X509CertificateChainModel {
                Status = model.Status?
                    .Select(s => (Core.Models.X509ChainStatus)s)
                    .ToList(),
                Chain = model.Chain?
                    .Select(c => c.ToServiceModel())
                    .ToList()
            };
        }
    }
}
