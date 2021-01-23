// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Extensions.Rpc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Endpoints hub
    /// </summary>
    [HubRoute("v3/endpoints/events")]
    [Authorize(Policy = Policies.CanRead)]
    public class EndpointsHub : Hub {

    }
}