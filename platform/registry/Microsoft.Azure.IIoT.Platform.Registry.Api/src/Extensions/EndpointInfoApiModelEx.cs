// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;

    /// <summary>
    /// Handle event
    /// </summary>
    public static class EndpointInfoApiModelEx {

        /// <summary>
        /// Update an endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="update"></param>
        public static EndpointInfoApiModel Patch(this EndpointInfoApiModel update,
            EndpointInfoApiModel endpoint) {
            if (update == null) {
                return endpoint;
            }
            if (endpoint == null) {
                endpoint = new EndpointInfoApiModel();
            }
            endpoint.ActivationState = update.ActivationState;
            endpoint.ApplicationId = update.ApplicationId;
            endpoint.EndpointState = update.EndpointState;
            endpoint.NotSeenSince = update.NotSeenSince;
            endpoint.AuthenticationMethods = update.AuthenticationMethods;
            endpoint.DiscovererId = update.DiscovererId;
            endpoint.Id = update.Id;
            endpoint.SecurityLevel = update.SecurityLevel;
            endpoint.Endpoint = (update.Endpoint ?? new EndpointApiModel())
                .Patch(endpoint.Endpoint);
            return endpoint;
        }
    }
}
