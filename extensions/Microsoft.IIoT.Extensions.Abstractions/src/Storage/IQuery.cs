// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Storage {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Lightweight queryable abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQuery<T> {

        /// <summary>
        /// Where predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Order
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        IQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector);

        /// <summary>
        /// Order
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        IQuery<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector);

        /// <summary>
        /// Project
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        IQuery<K> Select<K>(Expression<Func<T, K>> selector);

        /// <summary>
        /// Project many
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        IQuery<K> SelectMany<K>(Expression<Func<T, IEnumerable<K>>> selector);

        /// <summary>
        /// Limit to max records to return
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        IQuery<T> Take(int maxRecords = 1);

        /// <summary>
        /// Filter duplicates
        /// </summary>
        /// <returns></returns>
        IQuery<T> Distinct();

        /// <summary>
        /// Run query and return feed
        /// </summary>
        /// <returns></returns>
        IResultFeed<IDocumentInfo<T>> GetResults();

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> CountAsync(CancellationToken ct = default);
    }
}
