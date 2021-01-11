// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Tasks {

    /// <summary>
    /// Configuration for task processor
    /// </summary>
    public class TaskProcessorOptions {

        /// <summary>
        /// Max instances of processors that should run.
        /// </summary>
        public int MaxInstances { get; set; }

        /// <summary>
        /// Max queue size per processor
        /// </summary>
        public int MaxQueueSize { get; set; }
    }
}
