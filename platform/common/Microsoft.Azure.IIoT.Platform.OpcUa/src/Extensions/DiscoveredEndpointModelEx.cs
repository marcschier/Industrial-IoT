// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Models {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;
    using Microsoft.Azure.IIoT.Serializers;

    /// <summary>
    /// Discovered Endpoint Model extensions
    /// </summary>
    public static class DiscoveredEndpointModelEx {

        /// <summary>
        /// Create server model
        /// </summary>
        /// <param name="result"></param>
        /// <param name="hostAddress"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static ApplicationRegistrationModel ToServiceModel(this DiscoveredEndpointModel result,
            string hostAddress, IJsonSerializer serializer) {
            var type = result.Description.Server.ApplicationType.ToServiceType() ??
                ApplicationType.Server;
            return new ApplicationRegistrationModel {
                Application = new ApplicationInfoModel {

                    // Assigned at application processing time - leave null
                    ApplicationId = null,
                    DiscovererId = null,
                    GenerationId = null,
                    Created = null,
                    Updated = null,

                    ApplicationType = type,
                    ProductUri = result.Description.Server.ProductUri,
                    ApplicationUri = result.Description.Server.ApplicationUri,
                    DiscoveryUrls = new HashSet<string>(result.Description.Server.DiscoveryUrls),
                    DiscoveryProfileUri = result.Description.Server.DiscoveryProfileUri,
                    GatewayServerUri = result.Description.Server.GatewayServerUri,
                    HostAddresses = new HashSet<string> { hostAddress },
                    ApplicationName = result.Description.Server.ApplicationName?.Text,
                    Locale = result.Description.Server.ApplicationName?.Locale,
                    LocalizedNames = string.IsNullOrEmpty(result.Description.Server.ApplicationName.Locale) ?
                        null : new Dictionary<string, string> {
                            [result.Description.Server.ApplicationName.Locale] =
                                result.Description.Server.ApplicationName.Text
                        },
                    NotSeenSince = null,
                    Capabilities = new HashSet<string>(result.Capabilities)
                },
                Endpoints = new List<EndpointInfoModel> {
                    new EndpointInfoModel {
                        // Assigned at endpoint processing time - leave null
                        DiscovererId = null,
                        ApplicationId = null,
                        Id = null,
                        GenerationId = null,
                        NotSeenSince = null,
                        ActivationState = null,
                        EndpointState = null,

                        SecurityLevel = result.Description.SecurityLevel,
                        AuthenticationMethods = result.Description.UserIdentityTokens
                            .ToServiceModel(serializer),
                        EndpointUrl = result.Description.EndpointUrl, // Reported
                        Endpoint = new EndpointModel {
                            Url = result.AccessibleEndpointUrl, // Accessible
                            AlternativeUrls = new HashSet<string> {
                                result.AccessibleEndpointUrl,
                                result.Description.EndpointUrl,
                            },
                            Certificate = result.Description.ServerCertificate?.ToThumbprint(),
                            SecurityMode = result.Description.SecurityMode.ToServiceType() ??
                                SecurityMode.None,
                            SecurityPolicy = result.Description.SecurityPolicyUri
                        }
                    }
                }
            };
        }
    }
}
