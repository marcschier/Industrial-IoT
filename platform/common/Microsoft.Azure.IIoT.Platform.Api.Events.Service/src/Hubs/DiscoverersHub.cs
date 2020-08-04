// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service {
    using Microsoft.Azure.IIoT.Platform.Api.Events.Service.Auth;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Discovery hub
    /// </summary>
    [HubRoute("v2/discovery/events")]
    [Authorize(Policy = Policies.CanRead)]
    public class DiscoverersHub : Hub {

    }
}