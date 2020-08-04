// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Update settings on all module entities
    /// </summary>
    public abstract class AbstractRunHost : IHostProcess, IDisposable {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="startup"></param>
        /// <param name="logger"></param>
        /// <param name="taskName"></param>
        public AbstractRunHost(ILogger logger, string taskName, TimeSpan interval,
            TimeSpan? startup = null) {
            if (string.IsNullOrEmpty(taskName)) {
                throw new ArgumentNullException(nameof(taskName));
            }
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _startup = startup ?? TimeSpan.Zero;
            _interval = interval;
            _taskName = taskName;
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(StopAsync).Wait();
            OnDisposing();
        }

        /// <inheritdoc/>
        public Task StartAsync() {
            if (_current == null) {
                _current = new TimerTask(this);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync() {
            if (_current != null) {
                _current.Dispose();
                _current = null;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Run timer now
        /// </summary>
        protected void RunNow() {
            _current?.RunNow();
        }

        /// <summary>
        /// Disposing
        /// </summary>
        protected virtual void OnDisposing() { }

        /// <summary>
        /// Run the task operation
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task RunAsync(CancellationToken token);

        /// <summary>
        /// A continuously executing task that can be cancelled.
        /// </summary>
        private class TimerTask : IDisposable {

            /// <summary>
            /// Create task
            /// </summary>
            /// <param name="outer"></param>
            public TimerTask(AbstractRunHost outer) {
                _outer = outer;
                _timer = new Timer(OnTimerFiredAsync, _timer, _outer._startup,
                    Timeout.InfiniteTimeSpan);
            }

            /// <inheritdoc/>
            public void Dispose() {
                _cts.Cancel();
            }

            /// <summary>
            /// Run timer now
            /// </summary>
            internal void RunNow() {
                _timer.Change(0, Timeout.Infinite);
            }

            /// <summary>
            /// Timer operation
            /// </summary>
            /// <param name="sender"></param>
            private async void OnTimerFiredAsync(object sender) {
                try {
                    _cts.Token.ThrowIfCancellationRequested();
                    _outer._logger.Information("Running {taskName}.", _outer._taskName);
                    await _outer.RunAsync(_cts.Token);
                    _outer._logger.Information("{taskName} finished.", _outer._taskName);
                }
                catch (OperationCanceledException) {
                    // Cancel was called - dispose task
                    _cts.Dispose();
                    _timer.Dispose();
                    return;  // Completed
                }
                catch (Exception ex) {
                    _outer._logger.Error(ex, "Failed to run {taskName}.", _outer._taskName);
                }
                _timer.Change(_outer._interval, Timeout.InfiniteTimeSpan);
            }

#pragma warning disable IDE0069 // Disposable fields should be disposed
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly AbstractRunHost _outer;
            private readonly Timer _timer;
#pragma warning restore IDE0069 // Disposable fields should be disposed
        }

        private readonly ILogger _logger;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _startup;
        private readonly string _taskName;
        private TimerTask _current;
    }
}