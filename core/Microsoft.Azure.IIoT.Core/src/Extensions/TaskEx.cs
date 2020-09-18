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
            try {
                var result = await task;
                if (!condition(result)) {
                    return result;
                }
                return await fallback();
            }
            catch {
                return await fallback();
            }
        }

        /// <summary>
        /// Timeout after some time
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task<T> WithTimeoutOf<T>(this Task<T> task,
            TimeSpan timeout) {
#pragma warning restore IDE1006 // Naming Styles
            var cts = new CancellationTokenSource(timeout);
            return Task.Run(() => task, cts.Token);
        }

        /// <summary>
        /// Timeout after some time
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public static Task WithTimeoutOf(this Task task,
            TimeSpan timeout) {
#pragma warning restore IDE1006 // Naming Styles
            var cts = new CancellationTokenSource(timeout);
            return Task.Run(() => task, cts.Token);
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
