﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Utils {
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Triggered Worker process
    /// </summary>
    public class TaskTrigger : IAsyncDisposable {

        /// <summary>
        /// Create triggered task
        /// </summary>
        /// <param name="task"></param>
        public TaskTrigger(Func<CancellationToken, Task> task) {
            _worker = new Worker(async ct => {
                while (!ct.IsCancellationRequested) {
                    await _event.WaitAsync().ConfigureAwait(false);
                    if (!ct.IsCancellationRequested) {
                        await task(ct).ConfigureAwait(false);
                    }
                }
            });
        }

        /// <summary>
        /// Pull the trigger so worker executes
        /// </summary>
        public void Pull() {
            _event.Set();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            var result = _worker.DisposeAsync();
            _event.Set();
            await result.ConfigureAwait(false);
        }

        private readonly Worker _worker;
        private readonly AsyncAutoResetEvent _event = new AsyncAutoResetEvent();
    }
}