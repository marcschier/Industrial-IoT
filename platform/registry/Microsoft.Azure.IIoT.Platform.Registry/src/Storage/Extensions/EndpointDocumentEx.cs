// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
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
                SupervisorId = string.IsNullOrEmpty(document.SupervisorId) ?
                    null : document.SupervisorId,
                DiscovererId = string.IsNullOrEmpty(document.DiscovererId) ?
                    null : document.DiscovererId,
                AuthenticationMethods = document.AuthenticationMethods?.DecodeAsList(j =>
                    j.ConvertTo<AuthenticationMethodModel>()),
                SecurityLevel = document.SecurityLevel,
                EndpointUrl = string.IsNullOrEmpty(document.EndpointRegistrationUrl) ?
                    (string.IsNullOrEmpty(document.EndpointUrl) ?
                        document.EndpointUrlLC : document.EndpointUrl) : document.EndpointRegistrationUrl,
                Endpoint = new EndpointModel {
                    Url = string.IsNullOrEmpty(document.EndpointUrl) ?
                        document.EndpointUrlLC : document.EndpointUrl,
                    AlternativeUrls = document.AlternativeUrls?.DecodeAsList().ToHashSetSafe(),
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
        /// <param name="serializer"></param>
        /// <param name="disabled"></param>
        /// <param name="discovererId"></param>
        /// <param name="supervisorId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static EndpointDocument ToDocumentModel(this EndpointInfoModel model,
            IJsonSerializer serializer, bool? disabled = null, string discovererId = null,
            string supervisorId = null, string applicationId = null) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            if (serializer is null) {
                throw new ArgumentNullException(nameof(serializer));
            }

            return new EndpointDocument {
                IsDisabled = disabled,
                NotSeenSince = model.NotSeenSince,
                ApplicationId = applicationId ?? model.ApplicationId,
                SupervisorId = supervisorId ?? model.SupervisorId,
                DiscovererId = discovererId ?? model.DiscovererId,
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
    }
}
