// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Tasks.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Task processor configuration for runtime
    /// </summary>
    public class TaskProcessorConfig : PostConfigureOptionBase<TaskProcessorOptions> {

        /// <inheritdoc/>
        public TaskProcessorConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, TaskProcessorOptions options) {
            if (options.MaxInstances == 0) {
                options.MaxInstances = 1;
            }
            if (options.MaxQueueSize == 0) {
                options.MaxQueueSize = 1000;
            }
        }
    }
}
