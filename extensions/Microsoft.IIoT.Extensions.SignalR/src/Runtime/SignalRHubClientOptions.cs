// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Http.SignalR {

    /// <summary>
    /// Signalr client configuration
    /// </summary>
    public class SignalRHubClientOptions {

        /// <summary>
        /// Use message pack or json
        /// </summary>
        public bool UseMessagePackProtocol { get; set; }
    }
}