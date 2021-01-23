// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Message processing
    /// </summary>
    public interface IDataSetWriterMessageProcessor {

        /// <summary>
        /// Handle messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task HandleMessageAsync(PublishedDataSetItemMessageModel message);
    }
}
