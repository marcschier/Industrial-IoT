// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;

    /// <summary>
    /// Report state for writer group
    /// </summary>
    public interface IWriterGroupStateReporter {

        /// <summary>
        /// Report variable monitored item state
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void OnWriterGroupStateChange(string writerGroupId,
            WriterGroupStatus? state);
    }
}