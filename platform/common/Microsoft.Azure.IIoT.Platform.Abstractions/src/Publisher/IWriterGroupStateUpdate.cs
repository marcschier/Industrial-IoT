// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows update of writer group state
    /// </summary>
    public interface IWriterGroupStateUpdate {

        /// <summary>
        /// Update writer group state
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateWriterGroupStateAsync(string writerGroupId,
            WriterGroupState? state, PublisherOperationContextModel context = null,
            CancellationToken ct = default);
    }
}