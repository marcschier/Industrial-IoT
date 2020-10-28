// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http {
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    /// <summary>
    /// Http request extensions
    /// </summary>
    public static class HttpRequestEx {

        /// <summary>
        /// Set request timeout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IHttpRequest SetTimeout(this IHttpRequest request,
            TimeSpan? timeout) {
            request.Options.Set(kTimeoutKey, timeout);
            return request;
        }

        /// <summary>
        /// Get request timeout
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TimeSpan? GetTimeout(this IHttpRequest request) {
            if (!request.Options.TryGetValue(kTimeoutKey, out var timeout)) {
                return null;
            }
            return timeout;
        }

        /// <summary>
        /// Add header value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>this</returns>
        public static IHttpRequest AddHeader(this IHttpRequest request, string name,
            string value) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException(nameof(name));
            }

            if (!request.Headers.TryAddWithoutValidation(name, value)) {
                if (!name.Equals("content-type", StringComparison.InvariantCultureIgnoreCase)) {
                    throw new ArgumentOutOfRangeException(name, "Invalid header name");
                }
            }
            return request;
        }

        /// <summary>
        /// Set content
        /// </summary>
        /// <param name="request"></param>
        /// <param name="content"></param>
        /// <param name="mediaType"></param>
        /// <param name="encoding"></param>
        /// <returns>this</returns>
        public static IHttpRequest SetStringContent(this IHttpRequest request, string content,
            string mediaType = null, Encoding encoding = null) {
            if (encoding == null) {
                encoding = kDefaultEncoding;
            }
            return request.SetByteArrayContent(encoding.GetBytes(content),
                string.IsNullOrEmpty(mediaType) ? ContentMimeType.Json : mediaType, encoding);
        }

        /// <summary>
        /// Set content
        /// </summary>
        /// <param name="request"></param>
        /// <param name="content"></param>
        /// <param name="mediaType"></param>
        /// <param name="encoding"></param>
        /// <returns>this</returns>
        public static IHttpRequest SetByteArrayContent(this IHttpRequest request, byte[] content,
            string mediaType = null, Encoding encoding = null) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }

            var headerValue = new MediaTypeHeaderValue(
                string.IsNullOrEmpty(mediaType) ? ContentMimeType.Binary : mediaType);
            if (encoding != null) {
                headerValue.CharSet = encoding.WebName;
            }
            request.Content = new ByteArrayContent(content);
            request.Content.Headers.ContentType = headerValue;
            return request;
        }

        /// <summary>
        /// Set content from stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="content"></param>
        /// <param name="mediaType"></param>
        /// <param name="encoding"></param>
        /// <returns>this</returns>
        public static IHttpRequest SetStreamContent(this IHttpRequest request, Stream content,
            string mediaType = null, Encoding encoding = null) {
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }

            var headerValue = new MediaTypeHeaderValue(
                string.IsNullOrEmpty(mediaType) ? ContentMimeType.Binary : mediaType);
            if (encoding != null) {
                headerValue.CharSet = encoding.WebName;
            }
            request.Content = new StreamContent(content);
            request.Content.Headers.ContentType = headerValue;
            return request;
        }

        private static readonly HttpRequestOptionsKey<TimeSpan?> kTimeoutKey =
            new HttpRequestOptionsKey<TimeSpan?>("Timeout");
        private static readonly Encoding kDefaultEncoding = new UTF8Encoding();
    }
}
