// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Messaging {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event reader for simple event queues
    /// </summary>
    public interface IEventReader {

        /// <summary>
        /// Consume or cancel
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEnumerable<(byte[], IDictionary<string, string>)>> ReadAsync(
            CancellationToken ct = default);
    }
}