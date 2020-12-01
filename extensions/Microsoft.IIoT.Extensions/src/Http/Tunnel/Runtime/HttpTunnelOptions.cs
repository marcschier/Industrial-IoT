// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Http.Tunnel {

    /// <summary>
    /// Configuration for http tunnel usage
    /// </summary>
    public class HttpTunnelOptions {

        /// <summary>
        /// Dynamically configure use of tunnel or the use of
        /// regular http client
        /// </summary>
        public bool UseTunnel { get; set; }
    }
}
