// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.AspNetCore.Http {
    using Microsoft.IIoT.Extensions.Http;
    using System.Globalization;
    using System.Linq;
    using System;

    /// <summary>
    /// Http request extensions
    /// </summary>
    public static class HttpRequestEx {

        /// <summary>
        /// Get page size from header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static int? GetPageSize(this HttpRequest request, int? pageSize = null) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }

            if (pageSize == null &&
                request.Headers.ContainsKey(HttpHeader.MaxItemCount)) {
                return int.Parse(request.Headers[HttpHeader.MaxItemCount]
                    .FirstOrDefault(), CultureInfo.InvariantCulture);
            }
            return pageSize;
        }

        /// <summary>
        /// Get page size from header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public static string GetContinuationToken(this HttpRequest request,
            string continuationToken = null) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(continuationToken) &&
                request.Headers.ContainsKey(HttpHeader.ContinuationToken)) {
                continuationToken = request.Headers[HttpHeader.ContinuationToken]
                    .FirstOrDefault();
            }
            return continuationToken;
        }
    }
}
