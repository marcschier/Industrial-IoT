// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Models {
    using Microsoft.IIoT.Platform.Core.Api.Models;

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
            endpoint.ApplicationId = update.ApplicationId;
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
