// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus {

    /// <summary>
    /// Service bus queue processor configuration
    /// </summary>
    public interface IServiceBusProcessorConfig {

        /// <summary>
        /// Queue to process messages
        /// </summary>
        string Queue { get; }
    }
}
