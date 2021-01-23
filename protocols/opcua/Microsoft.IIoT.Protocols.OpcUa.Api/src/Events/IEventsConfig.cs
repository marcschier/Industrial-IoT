// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Events.Api {

    /// <summary>
    /// Configuration for service
    /// </summary>
    public interface IEventsConfig {

        /// <summary>
        /// Opc events service url
        /// </summary>
        string OpcUaEventsServiceUrl { get; }
    }
}
