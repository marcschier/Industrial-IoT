﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper to dispose a disposable and run cleanup
    /// </summary>
    public class AsyncDisposable : IAsyncDisposable {

        /// <summary>
        /// Create disposable
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="disposeAsync"></param>
        public AsyncDisposable(IDisposable disposable,
            Func<Task> disposeAsync = null) : this(disposeAsync) {
            _disposable = disposable;
        }

        /// <summary>
        /// Create disposable
        /// </summary>
        /// <param name="disposeAsync"></param>
        public AsyncDisposable(Func<Task> disposeAsync) {
            if (disposeAsync != null) {
                _disposeAsync = disposeAsync;
            }
            else {
                _disposeAsync = () => Task.CompletedTask;
            }
        }

        /// <summary>
        /// Create disposable
        /// </summary>
        /// <param name="disposables"></param>
        public AsyncDisposable(IEnumerable<IAsyncDisposable> disposables) {
            _disposeAsync = () => DisposeAsync(disposables);
        }

        /// <summary>
        /// Create from tasks
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task<IAsyncDisposable> AsAsyncDisposable(
            params Task<IAsyncDisposable>[] tasks) {
#pragma warning restore IDE1006 // Naming Styles
            return new AsyncDisposable(await WhenAll(tasks).ConfigureAwait(false));
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            if (_disposed) {
                throw new ObjectDisposedException("Async disposable already disposed");
            }
            _disposed = true;
            if (_disposable != null) {
                Try.Op(_disposable.Dispose);
            }
            if (_disposeAsync != null) {
                await _disposeAsync.Invoke().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Safe waiting for disposables
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<IAsyncDisposable[]> WhenAll(
            params Task<IAsyncDisposable>[] tasks) {
            return WhenAll((IEnumerable<Task<IAsyncDisposable>>)tasks);
        }

        /// <summary>
        /// Safe waiting for disposables
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task<IAsyncDisposable[]> WhenAll(
            IEnumerable<Task<IAsyncDisposable>> tasks) {
#pragma warning restore IDE1006 // Naming Styles
            try {
                return await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch {
                foreach (var task in tasks) {
                    if (task.IsCompleted) {
                        await Try.Async(() => task.Result.DisposeAsync().AsTask()).ConfigureAwait(false);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Safe waiting for disposables
        /// </summary>
        /// <param name="disposables"></param>
        /// <returns></returns>
        public static Task DisposeAsync(params IAsyncDisposable[] disposables) {
            return ((IEnumerable<IAsyncDisposable>)disposables).DisposeAsync();
        }

        /// <summary>
        /// Safe waiting for disposables
        /// </summary>
        /// <param name="disposables"></param>
        /// <returns></returns>
        public static Task DisposeAsync(IEnumerable<IAsyncDisposable> disposables) {
            return Try.Async(() => Task.WhenAll(disposables
                .Where(d => d != null)
                .Select(d => d.DisposeAsync().AsTask())));
        }

        private bool _disposed;
        private readonly IDisposable _disposable;
        private readonly Func<Task> _disposeAsync;
    }
}
