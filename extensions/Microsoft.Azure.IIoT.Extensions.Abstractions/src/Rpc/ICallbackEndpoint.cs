// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Rpc {
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// Hub Endpoint lookup
    /// </summary>
    public interface ICallbackEndpoint<THub> {

        /// <summary>
        /// Resource name
        /// </summary>
        string Resource { get; }

        /// <summary>
        /// Get client endpoint
        /// </summary>
        /// <returns>Client endpoint</returns>
        Uri EndpointUrl { get; }

        /// <summary>
        /// Creates a opaque access token for an identity to
        /// connect to Service.
        /// </summary>
        /// <param name="identity">The identity requesting access.
        /// </param>
        /// <param name="claims">The claim list to be put into
        /// identity token.</param>
        /// <param name="lifeTime">The lifetime of the token.
        /// </param>
        /// <returns>Client identity token</returns>
        string GenerateAccessToken(string identity = null,
            IList<Claim> claims = null, TimeSpan? lifeTime = default);
    }
}