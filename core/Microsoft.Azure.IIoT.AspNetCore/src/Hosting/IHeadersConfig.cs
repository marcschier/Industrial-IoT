// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Hosting {

    /// <summary>
    /// Forwarded headers processing configuration.
    /// </summary>
    public interface IHeadersConfig {

        /// <summary>
        /// Determines whethere processing of forwarded headers should be enabled or not.
        /// </summary>
        bool AspNetCoreForwardedHeadersEnabled { get; }

        /// <summary>
        /// Determines limit on number of entries in the forwarded headers.
        /// </summary>
        int AspNetCoreForwardedHeadersForwardLimit { get; }
    }
}
