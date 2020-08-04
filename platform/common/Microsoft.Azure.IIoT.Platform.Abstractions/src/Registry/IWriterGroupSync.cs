// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// Writer group registry synchronization
    /// </summary>
    public interface IWriterGroupSync {

        /// <summary>
        /// Sync writer group registration including contained writers
        /// </summary>
        /// <param name="writerGroup"></param>
        /// <param name="writers"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SynchronizeWriterGroupAsync(WriterGroupInfoModel writerGroup,
            IEnumerable<DataSetWriterInfoModel> writers,
            CancellationToken ct = default);
    }
}