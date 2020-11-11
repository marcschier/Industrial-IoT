// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {

    /// <summary>
    /// Higher level integration event bus implementation
    /// </summary>
    public interface IEventBus : IEventBusPublisher, IEventBusSubscriber {
    }
}
