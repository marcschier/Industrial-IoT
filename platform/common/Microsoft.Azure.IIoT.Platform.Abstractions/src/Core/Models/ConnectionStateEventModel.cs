// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {

    /// <summary>
    /// Connection state change event
    /// </summary>
    public class ConnectionStateEventModel {

        /// <summary>
        /// Connection id
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public ConnectionStateModel State { get; set; }
    }
}
