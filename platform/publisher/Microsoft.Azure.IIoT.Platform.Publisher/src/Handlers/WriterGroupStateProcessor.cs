// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Process reported state change messages and update entites in registry
    /// </summary>
    public sealed class WriterGroupStateProcessor : IWriterGroupStateProcessor {

        /// <summary>
        /// Create state processor service
        /// </summary>
        /// <param name="groups"></param>
        public WriterGroupStateProcessor(IWriterGroupStateUpdater groups) {
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }

        /// <summary>
        /// Handle state change
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnWriterGroupStateChangeAsync(WriterGroupStateEventModel message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var context = new OperationContextModel {
                Time = message.State?.LastStateChange ?? DateTime.UtcNow,
                AuthorityId = null // TODO
            };
            if (!string.IsNullOrEmpty(message.WriterGroupId)) {
                // Patch source state
                await _groups.UpdateWriterGroupStateAsync(
                   message.WriterGroupId, message.State.LastState, context).ConfigureAwait(false);
            }
        }

        private readonly IWriterGroupStateUpdater _groups;
    }
}
