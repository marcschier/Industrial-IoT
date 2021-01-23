// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Report state of connection
    /// </summary>
    public interface ITwinStateReporter {

        /// <summary>
        /// Report connection state
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void OnConnectionStateChange(string twinId,
            ConnectionStateModel state);
    }
}