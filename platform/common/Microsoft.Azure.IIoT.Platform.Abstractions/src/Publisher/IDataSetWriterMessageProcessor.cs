﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
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
