﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer group bulk operations
    /// </summary>
    public interface IWriterGroupBatchOperations {

        /// <summary>
        /// Import a writer group
        /// </summary>
        /// <param name="writerGroup"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ImportWriterGroupAsync(WriterGroupModel writerGroup,
            OperationContextModel context = null,
            CancellationToken ct = default);
    }
}
