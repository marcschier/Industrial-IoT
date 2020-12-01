// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Endpoint info
    /// </summary>
    public class EndpointInfoModel {

        /// <summary>
        /// Endpoint identifier which is hashed from
        /// the supervisor, site and url.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Discoverer that registered the endpoint
        /// </summary>
        public string DiscovererId { get; set; }

        /// <summary>
        /// Endpoint information in the registration
        /// </summary>
        public EndpointModel Endpoint { get; set; }

        /// <summary>
        /// Security level of the endpoint as advertised by server.
        /// </summary>
        public int? SecurityLevel { get; set; }

        /// <summary>
        /// Supported credential configurations that can be selected to
        /// obtain a credential and used to interact with the endpoint.
        /// </summary>
        public IReadOnlyList<AuthenticationMethodModel> AuthenticationMethods { get; set; }

        /// <summary>
        /// Application id endpoint is registered under.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Endpoint visibility
        /// </summary>
        public EntityVisibility? Visibility { get; set; }

        /// <summary>
        /// Last time endpoint was seen
        /// </summary>
        public DateTime? NotSeenSince { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        public OperationContextModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        public OperationContextModel Updated { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
