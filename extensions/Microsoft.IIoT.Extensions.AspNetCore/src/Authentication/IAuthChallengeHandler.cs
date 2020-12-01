// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.AspNetCore.Authentication {
    using Microsoft.AspNetCore.Http;
    using Microsoft.IIoT.Authentication.Models;
    using System.Security.Authentication;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle challenge
    /// </summary>
    public interface IAuthChallengeHandler {

        /// <summary>
        /// Handle authentication challenge
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resource"></param>
        /// <param name="provider"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        Task<TokenResultModel> ChallengeAsync(HttpContext context, string resource,
            string provider, AuthenticationException ex = null);
    }
}
