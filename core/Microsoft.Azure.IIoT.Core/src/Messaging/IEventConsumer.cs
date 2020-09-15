// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event consumer for simple event queues
    /// </summary>
    public interface IEventConsumer {

        /// <summary>
        /// Consume or cancel
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEnumerable<(string, byte[], IDictionary<string, string>)>> ConsumeAsync(
            CancellationToken ct = default);
    }
}