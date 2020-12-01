// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manual event
    /// </summary>
    public class AsyncEvent<T> {

        /// <summary>
        /// Whether event is set
        /// </summary>
        public bool IsSet => _tcs.Task.IsCompletedSuccessfully;

        /// <summary>
        /// Wait
        /// </summary>
        /// <returns></returns>
        public Task<T> WaitAsync() {
            return _tcs.Task;
        }

        /// <summary>
        /// Signal
        /// </summary>
        public void Set(T value) {
            _tcs.TrySetResult(value);
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset() {
            while (true) {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref _tcs,
                        new TaskCompletionSource<T>(
                            TaskCreationOptions.RunContinuationsAsynchronously), tcs) == tcs) {
                    return;
                }
            }
        }

        private volatile TaskCompletionSource<T> _tcs =
            new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
