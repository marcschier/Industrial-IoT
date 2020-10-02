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
            TimeSpan timeout) {
#pragma warning restore IDE1006 // Naming Styles
            using var cts = new CancellationTokenSource(timeout);
            return await Task.Run(() => task, cts.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Timeout after some time
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static async Task WithTimeoutOf(this Task task,
            TimeSpan timeout) {
#pragma warning restore IDE1006 // Naming Styles
            using var cts = new CancellationTokenSource(timeout);
            await Task.Run(() => task, cts.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Timeout after 1 minute
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task<T> With1MinuteTimeout<T>(this Task<T> task) {
#pragma warning restore IDE1006 // Naming Styles
            return task.WithTimeoutOf(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Timeout after 1 minute
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task With1MinuteTimeout(this Task task) {
#pragma warning restore IDE1006 // Naming Styles
            return task.WithTimeoutOf(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Timeout after 2 minutes
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task<T> With2MinuteTimeout<T>(this Task<T> task) {
#pragma warning restore IDE1006 // Naming Styles
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Timeout after 2 minutes
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task With2MinuteTimeout(this Task task) {
#pragma warning restore IDE1006 // Naming Styles
            return task.WithTimeoutOf(TimeSpan.FromMinutes(2));
        }
    }
}
