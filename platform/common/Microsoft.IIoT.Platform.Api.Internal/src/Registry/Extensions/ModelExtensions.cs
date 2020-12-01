// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api.Models {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using System.Linq;

    /// <summary>
    /// Model conversion extensions
    /// </summary>
    public static class ModelExtensions {

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewaySiteListApiModel ToApiModel(
            this GatewaySiteListModel model) {
            if (model == null) {
                return null;
            }
            return new GatewaySiteListApiModel {
                ContinuationToken = model.ContinuationToken,
                Sites = model.Sites?.ToList()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewaySiteListModel ToServiceModel(
            this GatewaySiteListApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewaySiteListModel {
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
        public static DiscovererApiModel ToApiModel(
            this DiscovererModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererApiModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                LogLevel = (TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DiscovererModel ToServiceModel(
            this DiscovererApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DiscovererListApiModel ToApiModel(
            this DiscovererListModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererListApiModel {
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
        public static DiscovererListModel ToServiceModel(
            this DiscovererListApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert To api model
        /// </summary>
        /// <returns></returns>
        public static DiscovererQueryApiModel ToApiModel(
            this DiscovererQueryModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererQueryApiModel {
                Connected = model.Connected,
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscovererQueryModel ToServiceModel(
            this DiscovererQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererQueryModel {
                Connected = model.Connected,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <returns></returns>
        public static DiscovererUpdateApiModel ToApiModel(
            this DiscovererUpdateModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererUpdateApiModel {
                GenerationId = model.GenerationId,
                LogLevel = (TraceLogLevel?)model.LogLevel,
            };
        }


        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static DiscovererUpdateModel ToServiceModel(
            this DiscovererUpdateApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiscovererUpdateModel {
                GenerationId = model.GenerationId,
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayApiModel ToApiModel(
            this GatewayModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayApiModel {
                Id = model.Id,
                SiteId = model.SiteId,
                Connected = model.Connected
            };
        }


        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayModel ToServiceModel(
            this GatewayApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayModel {
                Id = model.Id,
                SiteId = model.SiteId,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayInfoApiModel ToApiModel(
            this GatewayInfoModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayInfoApiModel {
                Gateway = model.Gateway.ToApiModel(),
                Modules = model.Modules.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayInfoModel ToServiceModel(
            this GatewayInfoApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayInfoModel {
                Gateway = model.Gateway.ToServiceModel(),
                Modules = model.Modules.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayModulesApiModel ToApiModel(
            this GatewayModulesModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayModulesApiModel {
                Publisher = model.Publisher.ToApiModel(),
                Supervisor = model.Supervisor.ToApiModel(),
                Discoverer = model.Discoverer.ToApiModel()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayModulesModel ToServiceModel(
            this GatewayModulesApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayModulesModel {
                Publisher = model.Publisher.ToServiceModel(),
                Supervisor = model.Supervisor.ToServiceModel(),
                Discoverer = model.Discoverer.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayListApiModel ToApiModel(
            this GatewayListModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayListApiModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayListModel ToServiceModel(
            this GatewayListApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayQueryApiModel ToApiModel(
            this GatewayQueryModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayQueryApiModel {
                SiteId = model.SiteId,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayQueryModel ToServiceModel(
            this GatewayQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayQueryModel {
                SiteId = model.SiteId,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayUpdateApiModel ToApiModel(
            this GatewayUpdateModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayUpdateApiModel {
                SiteId = model.SiteId,
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayUpdateModel ToServiceModel(
            this GatewayUpdateApiModel model) {
            if (model == null) {
                return null;
            }
            return new GatewayUpdateModel {
                SiteId = model.SiteId,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherApiModel ToApiModel(
            this PublisherModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherApiModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                LogLevel = (TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherModel ToServiceModel(
            this PublisherApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherListApiModel ToApiModel(
            this PublisherListModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherListApiModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create services model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherListModel ToServiceModel(
            this PublisherListApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherQueryApiModel ToApiModel(
            this PublisherQueryModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherQueryApiModel {
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherQueryModel ToServiceModel(
            this PublisherQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherQueryModel {
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherUpdateApiModel ToApiModel(
            this PublisherUpdateModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherUpdateApiModel {
                GenerationId = model.GenerationId,
                LogLevel = (TraceLogLevel?)model.LogLevel,
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherUpdateModel ToServiceModel(
            this PublisherUpdateApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublisherUpdateModel {
                GenerationId = model.GenerationId,
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel,
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DirectoryOperationApiModel ToApiModel(
            this OperationContextModel model) {
            if (model == null) {
                return null;
            }
            return new DirectoryOperationApiModel {
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
            this DirectoryOperationApiModel model) {
            if (model == null) {
                return null;
            }
            return new OperationContextModel {
                Time = model.Time,
                AuthorityId = model.AuthorityId
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorApiModel ToApiModel(
            this SupervisorModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorApiModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                LogLevel = (TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorModel ToServiceModel(
            this SupervisorApiModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorModel {
                Id = model.Id,
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel,
                OutOfSync = model.OutOfSync,
                Version = model.Version,
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorListApiModel ToApiModel(
            this SupervisorListModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorListApiModel {
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
        public static SupervisorListModel ToServiceModel(
            this SupervisorListApiModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorListModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorQueryApiModel ToApiModel(
            this SupervisorQueryModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorQueryApiModel {
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorQueryModel ToServiceModel(
            this SupervisorQueryApiModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorQueryModel {
                Connected = model.Connected
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorUpdateApiModel ToApiModel(
            this SupervisorUpdateModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorUpdateApiModel {
                LogLevel = (TraceLogLevel?)model.LogLevel
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorUpdateModel ToServiceModel(
            this SupervisorUpdateApiModel model) {
            if (model == null) {
                return null;
            }
            return new SupervisorUpdateModel {
                LogLevel = (Registry.Models.TraceLogLevel?)model.LogLevel
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
