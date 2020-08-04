// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Simple Worker process
    /// </summary>
    public class Worker : IAsyncDisposable {

        /// <summary>
        /// Create long running task
        /// </summary>
        /// <param name="task"></param>
        public Worker(Action<CancellationToken> task) {
            _runner = Task.Factory.StartNew(() => task(_cts.Token),
                _cts.Token, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Create long running task
        /// </summary>
        /// <param name="task"></param>
        public Worker(Func<CancellationToken, Task> task) {
            _runner = Task.Run(() => task(_cts.Token));
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            try {
                if (_runner == null) {
                    return;
                }
                _cts.Cancel();
                await _runner;
            }
            catch { }
            finally {
                _cts.Dispose();
            }
        }

        private readonly Task _runner;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    }
}