// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Threading.Tasks {

    /// <summary>
    /// Task extensions
    /// </summary>
    public static class TaskEx {

        /// <summary>
        /// Execute fallback task when this task fails.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task<T> FallbackWhen<T>(this Task<T> task,
            Func<T, bool> condition, Func<Task<T>> fallback) {
#pragma warning restore IDE1006 // Naming Styles
            if (task is null) {
                throw new ArgumentNullException(nameof(task));
            }
            try {
                var result = await task.ConfigureAwait(false);
                if (!(condition?.Invoke(result) ?? false)) {
                    return result;
                }
                return await fallback().ConfigureAwait(false);
            }
            catch {
                return await fallback().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Timeout after some time
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task<T> WithTimeoutOf<T>(this Task<T> task,
            TimeSpan timeout, Func<T> timeoutHandler = null) {
#pragma warning restore IDE1006 // Naming Styles
            var result = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);
            if (result != task) {
                if (timeoutHandler != null) {
                    return timeoutHandler();
                }
                throw new TimeoutException($"Timeout after {timeout}");
            }
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Timeout after some time
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task WithTimeoutOf(this Task task,
            TimeSpan timeout, Action timeoutHandler = null) {
#pragma warning restore IDE1006 // Naming Styles
            var result = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);
            if (result != task) {
                if (timeoutHandler != null) {
                    timeoutHandler();
                    return;
                }
                throw new TimeoutException($"Timeout after {timeout}");
            }
            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Timeout after 1 minute
        /// </summary>
        public static Task<T> With1MinuteTimeout<T>(this Task<T> task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Timeout after 1 minute
        /// </summary>
        public static Task With1MinuteTimeout(this Task task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Timeout after 2 minutes
        /// </summary>
        public static Task With2MinuteTimeout(this Task task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Timeout after 2 minutes
        /// </summary>
        public static Task<T> With2MinuteTimeout<T>(this Task<T> task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Timeout after 5 minutes
        /// </summary>
        public static Task With5MinuteTimeout(this Task task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Timeout after 5 minutes
        /// </summary>
        public static Task<T> With5MinuteTimeout<T>(this Task<T> task) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Continue after 1 minute
        /// </summary>
        public static Task ContinueAfter1Minutes(this Task task,
            Action handler = null) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2),
                () => handler?.Invoke());
        }

        /// <summary>
        /// Continue after 1 minutes
        /// </summary>
        public static Task<T> ContinueAfter1Minutes<T>(this Task<T> task,
            Func<T> handler = null) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(1),
                () => handler != null ? handler() : default);
        }

        /// <summary>
        /// Continue after 2 minutes
        /// </summary>
        public static Task ContinueAfter2Minutes(this Task task,
            Action handler = null) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2),
                () => handler?.Invoke());
        }

        /// <summary>
        /// Continue after 2 minutes
        /// </summary>
        public static Task<T> ContinueAfter2Minutes<T>(this Task<T> task,
            Func<T> handler = null) {
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2),
                () => handler != null ? handler() : default);
        }
    }
}
