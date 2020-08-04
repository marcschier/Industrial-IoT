// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Auth {
    using System.Threading.Tasks;

    /// <summary>
    /// Validates shared access token
    /// </summary>
    public interface ISasTokenValidator {

        /// <summary>
        /// Validate token and return identity.
        /// Throws if not valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> ValidateToken(string token);
    }
}