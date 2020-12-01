﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer group state event processing
    /// </summary>
    public interface IWriterGroupStateProcessor {

        /// <summary>
        /// Handle writer group state event messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnWriterGroupStateChangeAsync(WriterGroupStateEventModel message);
    }
}
