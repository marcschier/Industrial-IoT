// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Auth {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Creates shared access tokens
    /// </summary>
    public interface ISasTokenGenerator {

        /// <summary>
        /// Generate token for specified resource
        /// </summary>
        /// <param name="audience"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string> GenerateTokenAsync(string audience,
            CancellationToken ct = default);
    }
}
