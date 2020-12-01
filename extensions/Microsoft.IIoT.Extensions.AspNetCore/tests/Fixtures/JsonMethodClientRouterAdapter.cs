// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.IIoT.AspNetCore.Tests {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Rpc;
    using System;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Called by chunk server (typically the actual rpc client)
    /// </summary>
    public class JsonMethodClientRouterAdapter : IJsonMethodClient {

        public int MaxMethodPayloadSizeInBytes => 120 * 1024;

        public JsonMethodClientRouterAdapter(IMethodHandler router) {
            _router = router;
        }

        public async Task<string> CallMethodAsync(string target, string method,
            string payload, TimeSpan? timeout, CancellationToken ct) {
            var result = await _router.InvokeAsync(target, method,
                Encoding.UTF8.GetBytes(payload), ContentMimeType.Json).ConfigureAwait(false);
            const int kMaxMessageSize = 127 * 1024;
            if (result.Length > kMaxMessageSize) {
                throw new MethodCallStatusException(
                    (int)HttpStatusCode.RequestEntityTooLarge);
            }
            return Encoding.UTF8.GetString(result);
        }

        private readonly IMethodHandler _router;
    }

}
