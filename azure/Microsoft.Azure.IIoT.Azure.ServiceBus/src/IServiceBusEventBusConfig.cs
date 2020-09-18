// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus {

    /// <summary>
    /// Service bus event bus configuration
    /// </summary>
    public interface IServiceBusEventBusConfig {

        /// <summary>
        /// Shared topic to use
        /// </summary>
        string Topic { get; }
    }
}
