// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Result feed extensions
    /// </summary>
    public static class ResultFeedEx {

        /// <summary>
        /// Invoke callback for each element
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="callback"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task ForEachAsync<T>(this IResultFeed<T> feed,
            Func<T, Task> callback,
            CancellationToken ct = default) {
            while (feed.HasMore()) {
                var results = await feed.ReadAsync(ct);
                foreach (var item in results.ToList()) {
                    await callback(item);
                }
            }
        }

        /// <summary>
        /// Count results in feed
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync<T>(this IResultFeed<T> feed,
            CancellationToken ct = default) {
            var count = 0;
            while (feed.HasMore()) {
                var results = await feed.ReadAsync(ct);
                count += results.Count();
            }
            return count;
        }
    }
}
