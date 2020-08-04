// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
    /// </summary>
    public class AsyncEvent {

        /// <summary>
        /// Wait
        /// </summary>
        /// <returns></returns>
        public Task WaitAsync() {
            lock (_waits) {
                if (_signaled) {
                    _signaled = false;
                    return Task.CompletedTask;
                }
                else {
                    var tcs = new TaskCompletionSource<bool>();
                    _waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        /// <summary>
        /// Signal
        /// </summary>
        public void Set() {
            TaskCompletionSource<bool> toRelease = null;

            lock (_waits) {
                if (_waits.Count > 0) {
                    toRelease = _waits.Dequeue();
                }
                else if (!_signaled) {
                    _signaled = true;
                }
            }

            toRelease?.SetResult(true);
        }

        private readonly Queue<TaskCompletionSource<bool>> _waits =
            new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;
    }
}
