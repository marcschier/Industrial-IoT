// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Linq;

    /// <summary>
    /// Twin (endpoint) document extensions
    /// </summary>
    public static class EndpointDocumentEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="document"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static EndpointInfoModel ToServiceModel(this EndpointDocument document, string etag) {
            if (document == null) {
                return null;
            }
            return new EndpointInfoModel {
                ApplicationId = document.ApplicationId,
                GenerationId = etag,
                Id = document.Id,
                DiscovererId = string.IsNullOrEmpty(document.DiscovererId) ?
                    null : document.DiscovererId,
                AuthenticationMethods = document.AuthenticationMethods?.ToList(),
                SecurityLevel = document.SecurityLevel,
                EndpointUrl = string.IsNullOrEmpty(document.EndpointRegistrationUrl) ?
                    (string.IsNullOrEmpty(document.EndpointUrl) ?
                        document.EndpointUrlLC : document.EndpointUrl) : document.EndpointRegistrationUrl,
                Endpoint = new EndpointModel {
                    Url = string.IsNullOrEmpty(document.EndpointUrl) ?
                        document.EndpointUrlLC : document.EndpointUrl,
                    AlternativeUrls = document.AlternativeUrls?.ToHashSetSafe(),
                    SecurityMode = document.SecurityMode == SecurityMode.Best ?
                        null : document.SecurityMode,
                    SecurityPolicy = string.IsNullOrEmpty(document.SecurityPolicy) ?
                        null : document.SecurityPolicy,
                    Certificate = document.Thumbprint
                },
                ActivationState = document.ActivationState,
                NotSeenSince = document.NotSeenSince,
                EndpointState = document.ActivationState == EntityActivationState.Activated ?
                    (document.State == EndpointConnectivityState.Disconnected ?
                        EndpointConnectivityState.Connecting : document.State) :
                            EndpointConnectivityState.Disconnected
            };
        }

        /// <summary>
        /// Convert into document object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="disabled"></param>
        /// 
        /// <returns></returns>
        public static EndpointDocument ToDocumentModel(this EndpointInfoModel model,
            bool? disabled = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new EndpointDocument {
                IsDisabled = disabled,
                NotSeenSince = model.NotSeenSince,
                ApplicationId = model.ApplicationId,
                DiscovererId = model.DiscovererId,
                SecurityLevel = model.SecurityLevel,
                EndpointRegistrationUrl = model.EndpointUrl ??
                    model.Endpoint.Url,
                EndpointUrl = model.Endpoint.Url,
                AlternativeUrls = model.Endpoint.AlternativeUrls.ToHashSetSafe(),
                AuthenticationMethods = model.AuthenticationMethods?.ToList(),
                SecurityMode = model.Endpoint.SecurityMode ??
                    SecurityMode.Best,
                SecurityPolicy = model.Endpoint.SecurityPolicy,
                Thumbprint = model.Endpoint.Certificate,
                ActivationState = model.ActivationState
            };
        }
    }
}
