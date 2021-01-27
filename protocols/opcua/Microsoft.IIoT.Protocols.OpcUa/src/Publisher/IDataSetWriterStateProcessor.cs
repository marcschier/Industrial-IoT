﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer state event processing
    /// </summary>
    public interface IDataSetWriterStateProcessor {

        /// <summary>
        /// Handle dataset writer state event messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnDataSetWriterStateChangeAsync(DataSetWriterStateEventModel message);
    }
}