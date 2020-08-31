﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Message sink
    /// </summary>
    public interface IMessageSink {

        /// <summary>
        /// Messages sent so far
        /// </summary>
        ulong SentMessagesCount { get; }

        /// <summary>
        /// Send network message
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task SendAsync(IEnumerable<NetworkMessageModel> messages);
    }
}